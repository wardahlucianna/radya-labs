using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Storage;
using BinusSchool.Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Common.Functions.EventHub
{
    public class SyncReferenceTable : ISyncReferenceTable
    {
        private static readonly Lazy<EventHubProducerClientOptions> _producerClientOptions =
            new Lazy<EventHubProducerClientOptions>(
                new EventHubProducerClientOptions
                {
                    RetryOptions = new EventHubsRetryOptions
                    {
                        MaximumRetries = 5,
                        Delay = TimeSpan.FromSeconds(10),
                        Mode = EventHubsRetryMode.Exponential
                    }
                });

        private string _jsonChanges;
        private BinaryData _changesBinary;
        private EventData _changesEvent;
        private IStorageManager _storageManager;

        private readonly IConfiguration _configuration;
        private readonly IDictionary<string, string> _connectionStrings;
        private readonly ILogger<SyncReferenceTable> _logger;
        private readonly ILogger<StorageManager> _loggerStorageManager;
        private readonly ConcurrentDictionary<string, EventHubProducerClient> _eventHubProducer;
        private List<SyncRefTable> _changes { get; set; }

        public SyncReferenceTable(
            IConfiguration configuration,
            ILogger<SyncReferenceTable> logger,
            ILogger<StorageManager> loggerStorageManager)
        {
            _configuration = configuration;
            _connectionStrings = configuration.GetSection("ConnectionStrings:SyncRefTable:EventHubs")
                .Get<IDictionary<string, string>>();
            _logger = logger;
            _loggerStorageManager = loggerStorageManager;
            _eventHubProducer = new ConcurrentDictionary<string, EventHubProducerClient>();
        }

        public Task SendChanges(string dbName, IEnumerable<AuditChangeLog> changeLogs,
            IEnumerable<string> ignoreTablesToSync = null)
        {
            var auditStorageConnString =
                _configuration.GetSection("ConnectionStrings:Audit:AccountStorage").Get<string>();
            _storageManager = new StorageManager(auditStorageConnString, _loggerStorageManager);

            _changes = changeLogs
                .If(ignoreTablesToSync?.Any() ?? false,
                    x => x.Where(change => !ignoreTablesToSync.Contains(change.Table)))
                .Select(x => new SyncRefTable
                {
                    Table = x.Table,
                    Id = x.Id,
                    Action = x.Action,
                    Value = x.Value.ToDictionary(y => y.Key,
                        y => x.Action != AuditAction.Remove ? y.Value.New : y.Value.Old)
                }).ToList();
            if (!_changes.Any())
            {
                _logger.LogInformation("[EventHubs] No message will be sent to Event Hub. Synchronize done!");
                return Task.CompletedTask;
            }

            _jsonChanges = JsonConvert.SerializeObject(_changes);
            _changesBinary = new BinaryData(_jsonChanges);
            _changesEvent = new EventData(_changesBinary);

            // if part of db exist on other db, then send message about changes
            var dbRefs = _configuration.GetSection($"ConnectionStrings:{dbName}:Refs")
                .Get<IDictionary<string, string[]>>();

            // return immediately when dbRefs config is empty
            if (!(dbRefs?.Any(x => x.Value.Length != 0) ?? false))
            {
                _logger.LogInformation("[EventHubs] No message will be sent to Event Hub. Synchronize done!");
                return Task.CompletedTask;
            }

            var sendTasks = new List<Task>();
            foreach (var dbRef in dbRefs)
            {
                if (dbRef.Value.Length > 10)
                    throw new IndexOutOfRangeException("Max referrer is 10 on each Event Hub namespace.");

                // get connection string based on key of dbRef
                if (!_connectionStrings.TryGetValue(dbRef.Key, out var connectionString))
                    continue;
                // skip send to EventHub if connection string is empty
                if (string.IsNullOrEmpty(connectionString))
                    continue;

                foreach (var dbRefName in dbRef.Value)
                {
                    // send message to each referrer
                    var hubName = $"{dbName}-{dbRefName}".ToLower();

                    // cache EventHubProducerClient instance
                    if (!_eventHubProducer.ContainsKey(hubName))
                        _eventHubProducer.TryAdd(hubName,
                            new EventHubProducerClient(connectionString, hubName, _producerClientOptions.Value));

                    // add to batch execute task
                    sendTasks.Add(SendMessageToHub(dbRef.Key, hubName));
                }
            }

            // send all changes in one time
            return Task.WhenAll(sendTasks);
        }

        private async Task SendMessageToHub(string hubKey, string hubName)
        {
            try
            {
                var producerClient = _eventHubProducer[hubName];
                using var batchMessage = await producerClient.CreateBatchAsync();

                var total = 0;

                // set batch message per changes
                if (!batchMessage.TryAdd(_changesEvent))
                {
                    _logger.LogInformation(
                        "Message too large with total of {Total} data, will be saved in Blob storage instead of being sent to EventHub.",
                        _changes.Count);

                    var list = _changes.Chunkify(300);

                    foreach (var item in list)
                    {
                        total++;
                        // message too large, store to blob instead
                        var now = DateTimeUtil.ServerTime;
                        var blobName = $"{now:yyyy/MM/dd}/{Guid.NewGuid()}.json";
                        var blobContainerName = "sync-" + hubName;
                        var blobContainer = await _storageManager.GetOrCreateBlobContainer(blobContainerName);
                        var changesBinary = new BinaryData(JsonConvert.SerializeObject(item));
                        // upload changes to blob
                        var blobResult = await blobContainer.UploadBlobAsync(blobName, changesBinary);
                        var rawBlobResult = blobResult.GetRawResponse();
                        _logger.LogInformation("[Blob] {0} blob {1} in {2} container.", rawBlobResult.ReasonPhrase,
                            blobName, blobContainerName);

                        // create sync message based on uploaded blob
                        var changesBlob = new[]
                            { new MessageInBlob(_storageManager.AccountName, blobContainerName, blobName) };
                        var changesJson = JsonConvert.SerializeObject(changesBlob);
                        var changesEventBlob = new EventData(changesJson);
                        // add sync message with uploaded blob to batchMessage
                        batchMessage.TryAdd(changesEventBlob);
                    }

                    _logger.LogInformation("Message are split into multiple batches with size : {Total}", total);
                }
                else
                    total++;

                await producerClient.SendAsync(batchMessage);

                _logger.LogInformation(
                    "[EventHubs] Published synchronize message to hub ({0}) {1}, with total {2} message(s).",
                    hubName, // destination hub of sent message
                    hubKey, // key of event hub connection string
                    total
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EventHubs] {0}", ex.Message);
            }
        }
    }

    internal static class Helper
    {
        public static IEnumerable<IEnumerable<T>> Chunkify<T>(this IEnumerable<T> source, int size)
        {
            using var iter = source.GetEnumerator();
            while (iter.MoveNext())
            {
                var chunk = new T[size];
                var count = 1;
                chunk[0] = iter.Current;
                for (var i = 1; i < size && iter.MoveNext(); i++)
                {
                    chunk[i] = iter.Current;
                    count++;
                }

                if (count < size)
                {
                    Array.Resize(ref chunk, count);
                }

                yield return chunk;
            }
        }
    }
}
