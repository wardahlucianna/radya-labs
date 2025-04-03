using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues.Models;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Table.Entities;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BinusSchool.User.FnUser.Queue
{
    public class SaveNotificationHistoryHandler
    {
        private string _message;
        
        private readonly IUserDbContext _dbContext;
        private readonly IServiceProvider _provider;
        private readonly ILogger<SaveNotificationHistoryHandler> _logger;

        public SaveNotificationHistoryHandler(IUserDbContext dbContext, IServiceProvider provider, ILogger<SaveNotificationHistoryHandler> logger)
        {
            _dbContext = dbContext;
            _provider = provider;
            _logger = logger;
        }

        [FunctionName(nameof(SaveNotificationHistory))]
        public async Task SaveNotificationHistory(
            [QueueTrigger("save-notification")] string messageItem,
            CancellationToken cancellationToken)
        {
            var notificationHistory = default(Common.Model.NotificationHistory);
            var blobClient = default(BlobClient);
            var needToDeleteBlob = false;

            try
            {
                var jMessage = JToken.Parse(messageItem);

                if (jMessage.Value<bool>(nameof(MessageInBlob.StoredInBlob)))
                {
                    // message are stored in blob, need to fetch it first
                    var storageManager = _provider.GetService<IStorageManager>();

                    var blobName = jMessage.Value<string>(nameof(MessageInBlob.BlobName));
                    var blobContainerName = jMessage.Value<string>(nameof(MessageInBlob.BlobContainer));
                    var blobContainer =
                        await storageManager.GetOrCreateBlobContainer(blobContainerName, ct: cancellationToken);
                    blobClient = blobContainer.GetBlobClient(blobName);

                    await using var blobStream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
                    using var blobStreamReader = new StreamReader(blobStream);
                    using var jsonReader = new JsonTextReader(blobStreamReader);

                    while (await jsonReader.ReadAsync(cancellationToken))
                    {
                        if (jsonReader.TokenType == JsonToken.StartObject)
                        {
                            notificationHistory = new JsonSerializer().Deserialize<Common.Model.NotificationHistory>(jsonReader);
                            break;
                        }
                    }

                    // if operation above success, delete blob process will be executed after success save to database
                    needToDeleteBlob = true;
                }
                else
                {
                    notificationHistory = jMessage.ToObject<Common.Model.NotificationHistory>();
                }

                var notification = new TrNotification
                {
                    Id = Guid.NewGuid().ToString(),
                    IdSchool = notificationHistory!.IdSchool,
                    IdFeatureSchool = notificationHistory.IdFeature,
                    ScenarioNotificationTemplate = notificationHistory.Scenario,
                    Title = notificationHistory.Title,
                    Content = notificationHistory.Content,
                    IsBlast = notificationHistory.IsBlast,
                    Action = notificationHistory.Action,
                    NotificationType = notificationHistory.NotificationType,
                    Data = notificationHistory.Data
                };
                _dbContext.Entity<TrNotification>().Add(notification);

                foreach (var idUserRecipient in notificationHistory.IdUserRecipients)
                {
                    var userNotification = new TrNotificationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdNotification = notification.Id,
                        IdUser = idUserRecipient
                    };
                    _dbContext.Entity<TrNotificationUser>().Add(userNotification);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (JsonSerializationException serializeEx)
            {
                _logger.LogError(serializeEx, "[Queue] " + serializeEx.Message);
                SaveFailLog("JsonSerialization", serializeEx.Message, serializeEx.InnerException?.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Queue] {0}", ex.Message);
                SaveFailLog(notificationHistory?.FeatureCode ?? "Default", ex.Message, ex.InnerException?.Message);
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
        
        private void SaveFailLog(string featureCode, string errorMessage, string errorInnerMessage)
        {
            var failSend = new HsFailSaveNotificationHistory
            {
                PartitionKey = featureCode,
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
