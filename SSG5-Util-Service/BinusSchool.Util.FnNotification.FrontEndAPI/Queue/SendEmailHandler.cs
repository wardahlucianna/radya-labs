using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Table.Entities;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Util.FnNotification.Queue
{
    public class SendEmailHandler
    {
        private static readonly TrackingSettings _trackingSettings = new TrackingSettings
        {
            ClickTracking = new ClickTracking { Enable = true },
            OpenTracking = new OpenTracking { Enable = true }
        };
        
        private string _message;

        private readonly IServiceProvider _provider;
        private readonly ISendGridClient _sendGridClient;
        private readonly ILogger<SendEmailHandler> _logger;

        public SendEmailHandler(IServiceProvider provider)
        {
            _provider = provider;
            _sendGridClient = _provider.GetService<ISendGridClient>();
            _logger = _provider.GetService<ILogger<SendEmailHandler>>();
        }

        [FunctionName(nameof(SendEmailHandler.SendEmail))]
        public async Task SendEmail(
            [QueueTrigger("email-notification")] CloudQueueMessage queueMessage,
            CancellationToken cancellationToken)
        {
            var sendGridMessage = default(SendGridMessage);
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
                    var storageManager = _provider.GetService<IStorageManager>();

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
                            sendGridMessage = new JsonSerializer().Deserialize<SendGridMessage>(jsonReader);
                            break;
                        }
                    }

                    // if operation above success, delete blob process will be executed after success sending mail
                    needToDeleteBlob = true;
                }
                else
                {
                    sendGridMessage = jMessage.ToObject<SendGridMessage>();
                }

                // set fallback sender email when not provided
                if (sendGridMessage.From is null)
                {
                    var configuration = _provider.GetRequiredService<IConfiguration>();
                    var defaultSender = configuration.GetSection("EmailSender:Default").Get<EmailAddress>();

                    if (defaultSender is null || defaultSender.Email is null)
                        throw new ArgumentNullException(nameof(defaultSender));
                    
                    sendGridMessage.From = defaultSender;
                }
                // set sender email based on given idSchool
                else if (sendGridMessage.From is {} && !StringUtil.IsValidEmailAddress(sendGridMessage.From.Email))
                {
                    var configuration = _provider.GetRequiredService<IConfiguration>();
                    var schoolSender = configuration.GetSection($"EmailSender:{sendGridMessage.From.Email}").Get<EmailAddress>()
                        ?? configuration.GetSection("EmailSender:Default").Get<EmailAddress>();

                    if (schoolSender is null || schoolSender.Email is null)
                        throw new ArgumentNullException(nameof(schoolSender));
                    
                    sendGridMessage.From = schoolSender;
                }

                // add postfix environment for clarity
                //if (env != "Production")
                //    sendGridMessage.From.Name += $" ({env})";

                // set default tracker setting when not provided
                sendGridMessage.TrackingSettings ??= _trackingSettings;
                // add or set default categories (application environment)
                sendGridMessage.Categories ??= new List<string>();
                sendGridMessage.Categories.Add(env);

                // NOTE: intercept email repicipient
                //if (env == "Development")
                //{
                //    var configuration = _provider.GetRequiredService<IConfiguration>();
                //    if (configuration.GetValue<bool>($"InterceptEmail:{env}:Enable"))
                //    {
                //        var fakeRecipients = configuration.GetSection($"InterceptEmail:{env}:Recipients").Get<List<EmailAddress>>();

                //        foreach (var personalization in sendGridMessage.Personalizations)
                //        {
                //            personalization.Tos = fakeRecipients;
                //        }
                //    }
                //}

                var response = await _sendGridClient.SendEmailAsync(sendGridMessage, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[Queue] Sent Email: {0} from {1} to {2}",
                        sendGridMessage.Subject ?? "customized",
                        sendGridMessage.From.Email,
                        string.Join(", ",
                            sendGridMessage.Personalizations.SelectMany(x => x.Tos.Select(y => y.Email))));
                }
                else
                {
                    var responseMessage = await response.Body.ReadAsStringAsync();
                    throw new Exception(responseMessage);
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
                SaveFailLog(sendGridMessage?.From?.Email ?? "Anonymous", ex.Message, ex.InnerException?.Message);
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
            var failSend = new HsFailSendEmail
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
