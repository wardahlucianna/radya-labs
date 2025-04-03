using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Table.Entities;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using FluentEmail.MailKitSmtp;
using MailKit.Security;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Util.FnNotification.Queue
{
    public class SendSmtpHandler
    {
        private static readonly Lazy<JsonSerializer> _jsonSerializer = new Lazy<JsonSerializer>(JsonSerializer.Create(SerializerSetting.GetJsonSerializer(false)));

        private string _message;

        private readonly IServiceProvider _provider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendSmtpHandler> _logger;

        public SendSmtpHandler(IServiceProvider provider, IConfiguration configuration, ILogger<SendSmtpHandler> logger)
        {
            _provider = provider;
            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName(nameof(SendSmtp))]
        public async Task SendSmtp(
            [QueueTrigger("smtp-notification")] CloudQueueMessage queueMessage,
            CancellationToken cancellationToken)
        {
            var emailData = default(EmailData);
            var blobClient = default(BlobClient);
            var needToDeleteBlob = false;
            
            try
            {
                _message = queueMessage.AsString;
                _logger.LogInformation("[Queue] Dequeue message: {0} - {1}", queueMessage.Id, _message);

                var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
                var jMessage = JToken.Parse(_message);

                if (jMessage.Value<bool>(nameof(MessageInBlob.StoredInBlob)))
                {
                    // email content are stored in blob, need to fetch it first
                    var storageManager = _provider.GetRequiredService<IStorageManager>();

                    var blobName = jMessage.Value<string>(nameof(MessageInBlob.BlobName));
                    var blobContainerName = jMessage.Value<string>(nameof(MessageInBlob.BlobContainer));
                    var blobContainer = await storageManager.GetOrCreateBlobContainer(blobContainerName, ct: cancellationToken);
                    blobClient = blobContainer.GetBlobClient(blobName);

                    await using var blobStream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
                    using var blobStreamReader = new StreamReader(blobStream);
                    using var jsonReader = new JsonTextReader(blobStreamReader);

                    while (await jsonReader.ReadAsync(cancellationToken))
                    {
                        if (jsonReader.TokenType == JsonToken.StartObject)
                        {
                            emailData = _jsonSerializer.Value.Deserialize<EmailData>(jsonReader);
                            break;
                        }
                    }

                    // if operation above success, delete blob process will be executed after success sending mail
                    needToDeleteBlob = true;
                }
                else
                {
                    emailData = jMessage.ToObject<EmailData>(_jsonSerializer.Value);
                }
                
                // set fallback sender email when not provided
                if (emailData!.FromAddress is null)
                {
                    var configuration = _provider.GetRequiredService<IConfiguration>();
                    var defaultSender = configuration.GetSection("EmailSender:Default").Get<EmailAddress>();

                    if (defaultSender is null || defaultSender.Email is null)
                        throw new ArgumentNullException(nameof(defaultSender));
                    
                    emailData.FromAddress = new Address(defaultSender.Email, defaultSender.Name);
                }
                // set sender email based on given idSchool
                else if (emailData.FromAddress is {} && !StringUtil.IsValidEmailAddress(emailData.FromAddress.EmailAddress))
                {
                    var configuration = _provider.GetRequiredService<IConfiguration>();
                    var schoolSender = configuration.GetSection($"EmailSender:{emailData.FromAddress.EmailAddress}").Get<EmailAddress>()
                        ?? configuration.GetSection($"EmailSender:Default").Get<EmailAddress>();

                    if (schoolSender is null || schoolSender.Email is null)
                        throw new ArgumentNullException(nameof(schoolSender));

                    emailData.FromAddress = new Address(schoolSender.Email, schoolSender.Name);
                }
                
                //if (env != "Production")
                //{
                //    // add postfix environment for clarity
                //    emailData.FromAddress.Name ??= emailData.FromAddress.EmailAddress;
                //    emailData.FromAddress.Name += $" ({env})";
                //}
                
                // add or set default tags (application environment)
                emailData.Tags ??= new List<string>();
                emailData.Tags.Add(env);

                // setup mail sender
                var mailKit = new MailKitSender(new SmtpClientOptions
                {
                    UseSsl = true,
                    RequiresAuthentication = true,
                    SocketOptions = SecureSocketOptions.StartTls,
                    Server = _configuration.GetValue<string>("SmtpClient:Host"),
                    Port = _configuration.GetValue<int>("SmtpClient:Port"),
                    Password = _configuration.GetValue<string>("SmtpClient:Password"),
                    User = emailData.FromAddress.EmailAddress
                });

                var fluentEmail = new Email
                {
                    Data = emailData,
                    Sender = mailKit
                };

                // NOTE: intercept email repicipient
                //if (env != "Production")
                //{
                //    var configuration = _provider.GetRequiredService<IConfiguration>();
                //    if (configuration.GetValue<bool>($"InterceptEmail:{env}:Enable"))
                //    {
                //        var fakeRecipients = configuration.GetSection($"InterceptEmail:{env}:Recipients").Get<List<EmailAddress>>();
                //        fluentEmail.Data.ToAddresses = fakeRecipients.Select(x => new Address(x.Email, x.Name)).ToList();
                //    }
                //}
                
                var response = await fluentEmail.SendAsync(cancellationToken);
                
                if (response.Successful)
                {
                    _logger.LogInformation("[Queue] Sent Smtp: {0} from {1} to {2}",
                        emailData.Subject ?? "customized",
                        emailData.FromAddress.EmailAddress,
                        string.Join(", ", emailData.ToAddresses.Select(x => x.EmailAddress)));
                }
                else
                {
                    throw new Exception(string.Join(Environment.NewLine, response.ErrorMessages));
                }
            }
            catch (JsonSerializationException serializeEx)
            {
                _logger.LogError(serializeEx, "[Queue] " + serializeEx.Message);
                SaveFailLog("JsonSerialization", serializeEx.Message, serializeEx.InnerException?.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Queue] " + ex.Message);
                SaveFailLog(emailData?.FromAddress?.EmailAddress ?? "Anonymous", ex.Message, ex.InnerException?.Message);
            }
            finally
            {
                if (blobClient != null && needToDeleteBlob)
                {
                    var deleteResult = await blobClient
                        .DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
                    _logger.LogInformation("[Blob] Delete blob {0} status: {1} ({2}) in {3}", 
                        blobClient.Name, deleteResult.ReasonPhrase, deleteResult.Status, blobClient.BlobContainerName);
                }
            }
        }
        
        private void SaveFailLog(string sender, string errorMessage, string errorInnerMessage)
        {
            var failSend = new HsFailSendSmtp
            {
                PartitionKey = sender,
                RowKey = Guid.NewGuid().ToString(),
                Message = errorMessage,
                InnerMessage = errorInnerMessage,
                Value = _message
            };
            var tableManager = _provider.GetService<ITableManager>();
            _ = tableManager.InsertAndSave(failSend);
        }
    }
}
