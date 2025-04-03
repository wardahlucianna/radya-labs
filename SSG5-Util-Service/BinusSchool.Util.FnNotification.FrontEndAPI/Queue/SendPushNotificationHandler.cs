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
using FirebaseAdmin.Messaging;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Util.FnNotification.Queue
{
    public class SendPushNotificationHandler
    {
        private string _message;
        
        private readonly IServiceProvider _provider;
        private readonly ILogger<SendPushNotificationHandler> _logger;

        public SendPushNotificationHandler(IServiceProvider provider, ILogger<SendPushNotificationHandler> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        [FunctionName(nameof(SendPushNotification))]
        public async Task SendPushNotification(
            [QueueTrigger("push-notification")] CloudQueueMessage queueMessage,
            CancellationToken cancellationToken)
        {
            var multicastMessage = default(MulticastMessage);
            var blobClient = default(BlobClient);
            var needToDeleteBlob = false;

            try
            {
                _message = queueMessage.AsString;
                _logger.LogInformation("[Queue] Dequeue message: {0} - {1}", queueMessage.Id, _message);

                var jMessage = JToken.Parse(_message);

                if (jMessage.Value<bool>(nameof(MessageInBlob.StoredInBlob)))
                {
                    // message are stored in blob, need to fetch it first
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
                            multicastMessage = new JsonSerializer().Deserialize<MulticastMessage>(jsonReader);
                            break;
                        }
                    }

                    // if operation above success, delete blob process will be executed after success sending mail
                    needToDeleteBlob = true;
                }
                else
                {
                    multicastMessage = jMessage.ToObject<MulticastMessage>();
                }

                var (success, fail, responses) = (0, 0, new List<SendResponse>());
                foreach (var tokens in multicastMessage!.Tokens.Distinct().ChunkBy(1000))
                {
                    var message = new MulticastMessage
                    {
                        Tokens = tokens.ToList(),
                        Data = multicastMessage.Data,
                        Notification = multicastMessage.Notification
                    };
                    var fcmResponse = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message, cancellationToken);
                    
                    responses.AddRange(fcmResponse.Responses);
                    success += fcmResponse.SuccessCount;
                    fail += fcmResponse.FailureCount;
                }
                
                _logger.LogInformation("[Queue] Send Push notification: {0} with {1} success and {2} failed.", 
                    multicastMessage.Notification!.Title, success, fail);
                
                if (responses.Any(x => !x.IsSuccess))
                {
                    foreach (var response in responses.Select((v, i) => new { Value = v, Index = i }).Where(x => !x.Value.IsSuccess))
                    {
                        _logger.LogError("[FCM] Failed sent to {0}, {1} ({2}).", 
                            multicastMessage.Tokens[response.Index], 
                            response.Value.Exception.Message, 
                            response.Value.Exception.ErrorCode);
                  
                    }
                }
            }
            catch (JsonSerializationException serializeEx)
            {
                _logger.LogError(serializeEx, "[Queue] " + serializeEx.Message);
                SaveFailLog("JsonSerialization", serializeEx.Message, serializeEx.InnerException?.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Queue] {0}", ex.Message);
                SaveFailLog(multicastMessage?.Notification?.Title ?? "Default", ex.Message, ex.InnerException?.Message);
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
        
        private void SaveFailLog(string title, string errorMessage, string errorInnerMessage)
        {
            var failSend = new HsFailSendPushNotification
            {
                PartitionKey = title,
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
