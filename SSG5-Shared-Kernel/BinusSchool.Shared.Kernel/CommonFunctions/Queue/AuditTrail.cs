using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Storage;
using ByteSizeLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Common.Functions.Queue
{
    public class AuditTrail : IAuditTrail
    {
        public QueueClient QueueClient => _queueClient.Value;

        private static readonly string[] _excludeProps = new[] { "DateIn", "DateUp", "UserIn", "UserUp" };
        
        private readonly Lazy<QueueClient> _queueClient;
        private readonly Lazy<IStorageManager> _storageManager;
        private readonly ILogger<AuditTrail> _logger;

        public AuditTrail(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            var connectionString = configuration.GetConnectionString("Audit:AccountStorage");
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            _queueClient = new Lazy<QueueClient>(new QueueClient(connectionString, "audit-trail"));
            _storageManager = new Lazy<IStorageManager>(new StorageManager(connectionString, loggerFactory.CreateLogger<StorageManager>()));
            _logger = loggerFactory.CreateLogger<AuditTrail>();
        }

        public async Task SaveChangeLog(string dbName, string executor, DateTime time, IEnumerable<AuditChangeLog> changeLogs)
        {
            try
            {
                var logs = Enumerable.Empty<AuditLog>();
                foreach (var log in changeLogs)
                {
                    // exclude unchanged value & unneeded field
                    var logEntity = log.Value
                        .Where(x 
                            => !_excludeProps.Contains(x.Key)
                            && ((log.Action == AuditAction.Update && x.Value.Old != x.Value.New) || log.Action != AuditAction.Update))
                        .Select(x => new AuditLog(time, dbName, log.Table, x.Key, string.Join(',', log.Id), log.Action, executor, x.Value.Old, x.Value.New));
                    logs = logs.Concat(logEntity);
                }
                logs = logs.ToArray();

                if (logs.Any())
                {
                    await QueueClient.CreateIfNotExistsAsync();

                    // send queue message
                    var message = JsonConvert.SerializeObject(logs);
                    var messageBytes = Encoding.UTF8.GetBytes(message);

                    if (messageBytes.Length > SizeConstant.MaxQueueSize)
                    {
                        _logger.LogInformation("[Queue] Message too large, will be saved in Blob storage instead of being sent to Queue.");

                        // store changes value to blob
                        var blob = new BinaryData(messageBytes);
                        var blobName = $"{time:yyyy/MM/dd}/{executor}+{Guid.NewGuid()}.json";
                        var blobContainerName = "audit-" + dbName.ToLower();
                        var blobContainer = await _storageManager.Value.GetOrCreateBlobContainer(blobContainerName);
                        
                        // upload changes to blob
                        var blobResult = await blobContainer.UploadBlobAsync(blobName, blob);
                        var rawBlobResult = blobResult.GetRawResponse();
                        _logger.LogInformation("[Blob] {0} blob {1} in {2} container.", rawBlobResult.ReasonPhrase, blobName, blobContainer.Name);
                        
                        // override message with blob information
                        message = JsonConvert.SerializeObject(new[]
                        {
                            new MessageInBlob(_storageManager.Value.AccountName, blobContainerName, blobName, executor)
                        });
                        messageBytes = Encoding.UTF8.GetBytes(message);
                    }
                    
                    message = Convert.ToBase64String(messageBytes);
                    var receipt = await QueueClient.SendMessageAsync(message);
                    
                    _logger.LogInformation("[Queue] Queued changelog message: {0} for {1}.", 
                        receipt.Value.MessageId, 
                        string.Join(", ", logs.Select(x => x.Table).Distinct()));
                }
            }
            catch (Exception ex)
            {
                // TODO: handle when fail to queue change log message

                _logger.LogError(ex, "[Queue] " + ex.Message);
            }
        }
    }
}
