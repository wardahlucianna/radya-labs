using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Common.Storage;
using BinusSchool.Persistence.AuditDb.TableEntities;
using ByteSizeLib;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BinusSchool.Audit.FnAuditTrail.Queue
{
    public class LogAuditTrailHandler
    {
        private readonly CloudTable _cloudTable;
        private readonly IServiceProvider _provider;

        public LogAuditTrailHandler(IConfiguration configuration, IServiceProvider provider)
        {
            var connectionString = default(string);
#if DEBUG
            connectionString = "UseDevelopmentStorage=true;";
#else
            connectionString = configuration.GetConnectionString("Audit:TableStorage");
#endif

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudTableClient = storageAccount.CreateCloudTableClient();
            _cloudTable = cloudTableClient.GetTableReference(nameof(HsAuditTrail));
            _provider = provider;
        }

        [FunctionName(nameof(LogAuditTrailHandler.SaveLog))]
        public async Task SaveLog([QueueTrigger("audit-trail")] CloudQueueMessage queueMessage,
            CancellationToken cancellationToken,
            ILogger logger)
        {
            logger.LogInformation("[Queue] Dequeue message: {0} ({1}).", queueMessage.Id, ByteSize.FromBytes(queueMessage.AsBytes.Length).ToString("KB"));
            
            var blobClient = default(BlobClient);
            var isOperationSuccess = false;

            try
            {
                var logs = default(ICollection<AuditLog>);
                var jMessages = JArray.Parse(queueMessage.AsString);
                
                if (jMessages.Count == 0)
                    return;
                
                // check if changes are stored in blob
                if (jMessages[0].Value<bool>(nameof(MessageInBlob.StoredInBlob)))
                {
                    var blobName = jMessages[0].Value<string>(nameof(MessageInBlob.BlobName));
                    var blobContainerName = jMessages[0].Value<string>(nameof(MessageInBlob.BlobContainer));
                    
                    logger.LogInformation("[Blob] Reading blob {0} from {1} container.", blobName, blobContainerName);

#if DEBUG
                    var storageManager = new StorageManager("UseDevelopmentStorage=true", _provider.GetService<ILogger<StorageManager>>());
#else
                    var storageManager = _provider.GetService<IStorageManager>();
#endif
                    var blobContainer = await storageManager.GetOrCreateBlobContainer(blobContainerName, ct: cancellationToken);
                    blobClient = blobContainer.GetBlobClient(blobName);

                    await using var blobStream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
                    using var blobStreamReader = new StreamReader(blobStream);
                    using var jsonReader = new JsonTextReader(blobStreamReader);

                    while (await jsonReader.ReadAsync(cancellationToken))
                    {
                        if (jsonReader.TokenType == JsonToken.StartArray)
                        {
                            logs = new JsonSerializer().Deserialize<ICollection<AuditLog>>(jsonReader);
                            break;
                        }
                    }
                }
                else
                {
                    logs = jMessages.ToObject<ICollection<AuditLog>>();
                }

                await _cloudTable.CreateIfNotExistsAsync(cancellationToken);

                foreach (var logGroup in logs!.GroupBy(x => x.Id))
                {
                    var batchOperation = new TableBatchOperation();

                    foreach (var log in logGroup)
                    {
                        var audit = new HsAuditTrail
                        {
                            RowKey = Guid.NewGuid().ToString(),
                            PartitionKey = log.Id,
                            Timestamp = log.Time,
                            Action = log.Action,
                            Database = log.Database,
                            Table = log.Table,
                            Column = log.Column,
                            Executor = log.Executor,
                            OldValue = log.OldValue,
                            NewValue = log.NewValue
                        };

                        var operation = TableOperation.Insert(audit);
                        batchOperation.Add(operation);
                    }

                    var result = await _cloudTable.ExecuteBatchAsync(batchOperation, cancellationToken);
                    if (result.RequestCharge.HasValue)
                    {
                        logger.LogInformation(
                            "[Cosmos] Request charge of Insert operation of PartitionKey: {0} is {1}", 
                            logGroup.Key,
                            result.RequestCharge);
                    }
                }

                isOperationSuccess = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                // delete blob after success write to AuditTrail
                if (blobClient != null && isOperationSuccess && (await blobClient.ExistsAsync(cancellationToken)).Value)
                {
                    var deleteResult = await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
                    logger.LogInformation("[Blob] Deleted ({0}) blob {1} in {2} container.", deleteResult.Status, blobClient.Name, blobClient.BlobContainerName);
                }
            }
        }
    }
}
