using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Table.Entities;
using BinusSchool.Common.Model;
using BinusSchool.Common.Storage;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Configurations;
using BinusSchool.Data.Model.School.FnSchool.NotificationTemplate;
using ByteSizeLib;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Common.Functions.Queue
{
    public class NotificationManager : INotificationManager
    {
        private IStorageManager _userStorageManager;
        private IStorageManager _utilStorageManager;

        private readonly string _userStorageConnString;
        private readonly string _utilStorageConnString;
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationManager> _logger;

        public NotificationManager(IServiceProvider provider, ILogger<NotificationManager> logger)
        {
            _provider = provider;
            _logger = logger;

            _configuration = _provider.GetService<IConfiguration>();
            _userStorageConnString = _configuration.GetConnectionString("User:AccountStorage");
            _utilStorageConnString = _configuration.GetConnectionString("Util:AccountStorage");

            // NOTE: for testing locally, uncomment this to use storage emulator instead of Azure Storage Account
            // _userStorageConnString = _utilStorageConnString = "UseDevelopmentStorage=true";

            if (string.IsNullOrEmpty(_userStorageConnString))
                throw new ArgumentNullException(nameof(_userStorageConnString));
            if (string.IsNullOrEmpty(_utilStorageConnString))
                throw new ArgumentNullException(nameof(_utilStorageConnString));
        }

        public virtual async Task<NotificationTemplate> GetTemplate(string idSchool, string idScenario)
        {
            // TODO: find the best way to get notification template, either using storage or cache

            bool.TryParse(Environment.GetEnvironmentVariable(EnvironmentConstant.UseHttpClientFactory), out var useHttpClientFactory);
            var param = new GetNotificationTemplateScenarioRequest
            {
                IdSchool = idSchool,
                IdScenario = idScenario
            };

            var result = useHttpClientFactory
                ? await _provider.GetRequiredService<INotificationTemplate>().GetNotificationTemplateScenario(param)
                : await _provider.GetRequiredService<IApiService<INotificationTemplate>>()
                    .SetConfigurationFrom(_configuration.GetSection("BinusSchoolService").Get<BinusSchoolApiConfiguration2>())
                    .Execute.GetNotificationTemplateScenario(param);

            if (!result.IsSuccess)
                throw new Exception(result.Message, new Exception(result.InnerMessage));

            return result.Payload;
        }

        public async Task SaveNotification(NotificationHistory notification)
        {
            var jsonMessage = default(string);
            var blobContainerName = default(string);

            try
            {
                jsonMessage = JsonConvert.SerializeObject(notification);
                var queueClient = new QueueClient(_userStorageConnString, "save-notification");
                await queueClient.CreateIfNotExistsAsync();

                var bytesMessage = Encoding.UTF8.GetBytes(jsonMessage);
                var base64Message = default(string);

                // store to blob when message len more than 64 KiB
                if (bytesMessage.Length >= SizeConstant.MaxQueueSize)
                {
                    blobContainerName = $"save-{notification.Scenario.ToLower()}";
                    var blobName = await UploadMessageToBlob(StorageDestination.User, blobContainerName, jsonMessage);

                    // create queue message based on uploaded blob
                    var messageBlob = new MessageInBlob(_userStorageManager.AccountName, blobContainerName, blobName);
                    var messageJson = JsonConvert.SerializeObject(messageBlob);
                    bytesMessage = Encoding.UTF8.GetBytes(messageJson);
                }

                base64Message = Convert.ToBase64String(bytesMessage);
                var receipt = await queueClient.SendMessageAsync(base64Message);

                _logger.LogInformation("[Queue] Queued send notification history: {0} for {1} feature", receipt.Value.MessageId, notification.Scenario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Queue] " + ex.Message);

                var failQueue = new HsFailQueueNotificationHistory
                {
                    PartitionKey = blobContainerName ?? "save-default",
                    RowKey = Guid.NewGuid().ToString(),
                    Message = ex.Message,
                    InnerMessage = ex.InnerException?.Message,
                    Value = jsonMessage
                };
                var tableManager = _provider.GetService<ITableManager>();
                _ = tableManager.InsertAndSave(failQueue);

                throw;
            }
        }

        public async Task<string> SendEmail(SendGridMessage message)
        {
            var jsonMessage = default(string);
            var blobContainerName = default(string);

            try
            {
                if (!string.IsNullOrWhiteSpace(message.PlainTextContent) || !string.IsNullOrWhiteSpace(message.HtmlContent))
                {
                    message.Contents ??= new List<Content>();
                    if (!string.IsNullOrWhiteSpace(message.PlainTextContent))
                        message.Contents.Add(new Content("text/plain", message.PlainTextContent));
                    if (!string.IsNullOrWhiteSpace(message.HtmlContent))
                        message.Contents.Add(new Content("text/html", message.HtmlContent));
                }

                var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");

                #region bccs
                if (env == "Staging")
                {
                    List<EmailAddress> emailBccs = new List<EmailAddress>();
                    // emailBccs.Add(new EmailAddress("bsslog.prod@gmail.com", "Bcc Staging"));
                    // emailBccs.Add(new EmailAddress("theodorus.v@binus.edu", "Bcc Staging"));
                    // emailBccs.Add(new EmailAddress("dlugiana@binus.edu", "Bcc Staging"));
                    emailBccs.Add(new EmailAddress("itdevschool@binus.edu", "Bcc Staging"));
                    message.AddBccs(emailBccs);
                }

                if (env == "Production")
                {
                    List<EmailAddress> emailBccs = new List<EmailAddress>();
                    // emailBccs.Add(new EmailAddress("bsslog.prod@gmail.com", "Bcc Production"));
                    // emailBccs.Add(new EmailAddress("theodorus.v@binus.edu", "Bcc Production"));
                    // emailBccs.Add(new EmailAddress("dlugiana@binus.edu", "Bcc Production"));
                    emailBccs.Add(new EmailAddress("itdevschool@binus.edu", "Bcc Staging"));
                    message.AddBccs(emailBccs);
                }
                # endregion

                jsonMessage = JsonConvert.SerializeObject(message);
                var queueClient = new QueueClient(_utilStorageConnString, "email-notification");
                await queueClient.CreateIfNotExistsAsync();

                var bytesMessage = Encoding.UTF8.GetBytes(jsonMessage);
                var base64Message = default(string);

                // store to blob when message len more than 64 KiB
                if (bytesMessage.Length >= SizeConstant.MaxQueueSize)
                {
                    blobContainerName = $"email-{message.From?.Email ?? "default"}";
                    var blobName = await UploadMessageToBlob(StorageDestination.Util, blobContainerName, jsonMessage);

                    // create queue message based on uploaded blob
                    var messageBlob = new MessageInBlob(_utilStorageManager.AccountName, blobContainerName, blobName);
                    var messageJson = JsonConvert.SerializeObject(messageBlob);
                    bytesMessage = Encoding.UTF8.GetBytes(messageJson);
                }

                base64Message = Convert.ToBase64String(bytesMessage);
                var receipt = await queueClient.SendMessageAsync(base64Message);

                _logger.LogInformation("[Queue] Queued send email: {0} with {1} subject", receipt.Value.MessageId, message.Subject ?? "customized");

                return receipt.Value.MessageId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Queue] " + ex.Message);

                var failQueue = new HsFailQueueEmail
                {
                    PartitionKey = blobContainerName ?? "email-default",
                    RowKey = Guid.NewGuid().ToString(),
                    Message = ex.Message,
                    InnerMessage = ex.InnerException?.Message,
                    Value = jsonMessage
                };
                var tableManager = _provider.GetService<ITableManager>();
                _ = tableManager.InsertAndSave(failQueue);

                throw;
            }
        }

        public async Task<string> SendSmtp(EmailData message)
        {
            if (message is null)
                throw new ArgumentException("Email data must not be empty");

            var jsonMessage = default(string);
            var blobContainerName = default(string);

            try
            {
                var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");

                #region bccs
                if (env == "Staging")
                {
                    message.BccAddresses.Add(new Address("bsslog.prod@gmail.com", "Bcc Staging"));
                }

                if (env == "Production")
                {
                    message.BccAddresses.Add(new Address("bsslog.prod@gmail.com", "Bcc Production"));
                }
                # endregion


                jsonMessage = JsonConvert.SerializeObject(message, SerializerSetting.GetJsonSerializer(false));
                var queueClient = new QueueClient(_utilStorageConnString, "smtp-notification");
                await queueClient.CreateIfNotExistsAsync();

                var bytesMessage = Encoding.UTF8.GetBytes(jsonMessage);
                var base64Message = default(string);

                // store to blob when message len more than 64 KiB
                if (bytesMessage.Length >= SizeConstant.MaxQueueSize)
                {
                    blobContainerName = $"smtp-{message.FromAddress?.EmailAddress ?? "default"}";
                    var blobName = await UploadMessageToBlob(StorageDestination.Util, blobContainerName, jsonMessage);

                    // create queue message based on uploaded blob
                    var messageBlob = new MessageInBlob(_utilStorageManager.AccountName, blobContainerName, blobName);
                    var messageJson = JsonConvert.SerializeObject(messageBlob, SerializerSetting.GetJsonSerializer(false));
                    bytesMessage = Encoding.UTF8.GetBytes(messageJson);
                }

                base64Message = Convert.ToBase64String(bytesMessage);
                var receipt = await queueClient.SendMessageAsync(base64Message);

                _logger.LogInformation("[Queue] Queued send smtp: {0} with {1} subject", receipt.Value.MessageId, message.Subject ?? "customized");

                return receipt.Value.MessageId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Queue] " + ex.Message);

                var failQueue = new HsFailQueueSmtp
                {
                    PartitionKey = blobContainerName ?? "smtp-default",
                    RowKey = Guid.NewGuid().ToString(),
                    Message = ex.Message,
                    InnerMessage = ex.InnerException?.Message,
                    Value = jsonMessage
                };
                var tableManager = _provider.GetService<ITableManager>();
                _ = tableManager.InsertAndSave(failQueue);

                throw;
            }
        }

        public async Task<string> SendPushNotification(MulticastMessage message)
        {
            if (message.Tokens is null || message.Tokens.Count == 0)
                throw new ArgumentException("Push token must not be empty");

            var jsonMessage = default(string);
            var blobContainerName = default(string);

            try
            {
                jsonMessage = JsonConvert.SerializeObject(message);
                var queueClient = new QueueClient(_utilStorageConnString, "push-notification");
                await queueClient.CreateIfNotExistsAsync();

                var bytesMessage = Encoding.UTF8.GetBytes(jsonMessage);
                var base64Message = default(string);

                // store to blob when message len more than 64 KiB
                if (bytesMessage.Length >= SizeConstant.MaxQueueSize)
                {
                    blobContainerName = $"push-{message.Notification.Title.ToLowerInvariant().Replace(" ", "-")}";
                    var blobName = await UploadMessageToBlob(StorageDestination.Util, blobContainerName, jsonMessage);

                    // create queue message based on uploaded blob
                    var messageBlob = new MessageInBlob(_utilStorageManager.AccountName, blobContainerName, blobName);
                    var messageJson = JsonConvert.SerializeObject(messageBlob);
                    bytesMessage = Encoding.UTF8.GetBytes(messageJson);
                }

                base64Message = Convert.ToBase64String(bytesMessage);
                var receipt = await queueClient.SendMessageAsync(base64Message);

                _logger.LogInformation("[Queue] Queued send push notification: {0} with {1} title", receipt.Value.MessageId, message.Notification.Title);

                return receipt.Value.MessageId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Queue] " + ex.Message);

                var failQueue = new HsFailQueuePushNotif
                {
                    PartitionKey = blobContainerName ?? "push-default",
                    RowKey = Guid.NewGuid().ToString(),
                    Message = ex.Message,
                    InnerMessage = ex.InnerException?.Message,
                    Value = jsonMessage
                };
                var tableManager = _provider.GetService<ITableManager>();
                _ = tableManager.InsertAndSave(failQueue);

                throw;
            }
        }

        private async Task<string> UploadMessageToBlob(StorageDestination destination, string blobContainerName, string jsonMessage)
        {
            var storageManager = destination switch
            {
                StorageDestination.User => _userStorageManager ??= new StorageManager(_userStorageConnString, _provider.GetService<ILogger<StorageManager>>()),
                StorageDestination.Util => _utilStorageManager ??= new StorageManager(_utilStorageConnString, _provider.GetService<ILogger<StorageManager>>()),
                _ => default(IStorageManager)
            };

            // create blob
            var blobName = $"{DateTimeUtil.ServerTime:yyyy/MM/dd}/{Guid.NewGuid()}.json";
            var blobContainer = await storageManager!.GetOrCreateBlobContainer(blobContainerName);

            // upload message to blob
            var blobResult = await blobContainer.UploadBlobAsync(blobName, new BinaryData(jsonMessage));
            var rawBlobResult = blobResult.GetRawResponse();
            _logger.LogInformation("[Blob] Blob {0} status: {1} in {2}", blobResult, rawBlobResult.ReasonPhrase, blobContainerName);

            return blobName;
        }
    }

    internal enum StorageDestination
    {
        User,
        Util
    }
}
