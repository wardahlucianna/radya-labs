using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Common.Functions.Table
{
    public class TableManager : ITableManager
    {
        public CloudTableClient TableClient => _tableClient.Value;
        
        private readonly Lazy<CloudTableClient> _tableClient;
        private readonly ILogger<TableManager> _logger;

        public TableManager(IConfiguration configuration, ICurrentFunctions currentFunctions, ILogger<TableManager> logger)
        {
            var connectionString = configuration.GetConnectionString($"{currentFunctions.Domain}:AccountStorage");

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            _tableClient = new Lazy<CloudTableClient>(storageAccount.CreateCloudTableClient());
            _logger = logger;
        }

        public async Task InsertAndSave<T>(IEnumerable<T> entries, CancellationToken ct = default) where T : ITableEntity
        {
            try
            {
                var cloudTable = TableClient.GetTableReference(typeof(T).Name);
                await cloudTable.CreateIfNotExistsAsync(ct);
                var batchOperation = new TableBatchOperation();
                
                entries = entries.ToArray();
                foreach (var entry in entries)
                {
                    var operation = TableOperation.Insert(entry);
                    batchOperation.Add(operation);
                }

                var result = await cloudTable.ExecuteBatchAsync(batchOperation, ct);
                _logger.LogInformation("[Table] Inserted {0} entry to {1} (request charge: {2}).",
                    result.Count,
                    string.Join(", ", entries.Select(x => x.GetType().Name).Distinct()),
                    result.RequestCharge ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Table] " + ex.Message);
            }
        }

        public async Task InsertAndSave<T>(T entry, CancellationToken ct = default) where T : ITableEntity
        {
            try
            {
                var cloudTable = TableClient.GetTableReference(typeof(T).Name);
                await cloudTable.CreateIfNotExistsAsync(ct);

                var operation = TableOperation.Insert(entry);
                var result = await cloudTable.ExecuteAsync(operation, ct);
                
                _logger.LogInformation("[Table] Inserted an entry to {0} with PK: {1} and RK: {2} (request charge: {3}).",
                    typeof(T).Name, entry.PartitionKey, entry.RowKey, result.RequestCharge ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Table] " + ex.Message);
            }
        }
    }
}
