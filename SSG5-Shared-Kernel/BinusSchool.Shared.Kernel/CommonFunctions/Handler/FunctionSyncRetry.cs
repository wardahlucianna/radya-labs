using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Repositories;
using BinusSchool.Common.Storage;
using BinusSchool.Domain.Abstractions;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BinusSchool.Common.Functions.Handler
{
    public class FunctionSyncRetry<T>
        where T : DbContext, IAppDbContext
    {
        private readonly string _domain;
        private const int _maxRetry = 3;
        private const int _take = 2;

        public FunctionSyncRetry(string domain)
        {
            _domain = domain;
        }

        public async Task RunAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            var dbContext = scope.ServiceProvider.GetService<T>();
            var logger = scope.ServiceProvider.GetService<ILogger<FunctionSyncRetry<T>>>();
            var configuration = scope.ServiceProvider.GetService<IConfiguration>();
            var cosmosConnString = configuration.GetConnectionString("Sync:CosmosDbNoSql");
            var cosmosClient = new CosmosClient(cosmosConnString);
            var syncTableRepository = new CollectionOfRepositories.SyncTableRepository(cosmosClient);
            var syncTableHistoryRepository = new CollectionOfRepositories.SyncTableHistoryRepository(cosmosClient);
            var auditStorageConnString = configuration.GetSection("ConnectionStrings:Audit:AccountStorage")
                .Get<string>();
            var storageManager = new StorageManager(auditStorageConnString,
                scope.ServiceProvider.GetService<ILogger<StorageManager>>());

            logger.LogInformation("Execute check retry sync from Domain : {Domain}", _domain);

            var data = await syncTableRepository
                .GetAsync(predicate: e =>
                        e.Domain == _domain
                        && e.IsFixed == false
                    , orderBy: e => e.OrderBy(y => y.CreatedDate)
                    , usePaging: true
                    , pageSize: 10);

            var totalData = data.Items.Count();
            logger.LogInformation("Default per execution is 3 but total data : {total}", totalData);
            if (totalData == 0)
                return;

            foreach (var item in data.Items.Take(_take))
            {
                try
                {
                    logger.LogInformation("Iteration sync table id : {id}", item.Id);

                    if (string.IsNullOrWhiteSpace(item.Filename))
                    {
                        item.IsFixed = true;
                        item.FixedDt = DateTime.UtcNow;
                        item.FixedMessage = "override because filename is empty, either data testing or invalid data";
                        await syncTableRepository.UpdateAsync(item.Id, item);
                        continue;
                    }

                    logger.LogInformation("Reading blob {BlobName} from {BlobContainerName} container",
                        item.Filename, item.Container);

                    var blobContainer =
                        await storageManager.GetOrCreateBlobContainer(item.Container, ct: cancellationToken);
                    var blobClient = blobContainer.GetBlobClient(item.Filename);
                    var blobExists = await blobClient.ExistsAsync(cancellationToken);
                    if (blobExists.Value == false)
                    {
                        item.IsFixed = true;
                        item.FixedDt = DateTime.UtcNow;
                        item.FixedMessage = $"File blob does not exist anymore in the storage";
                        await syncTableRepository.UpdateAsync(item.Id, item);
                        continue;
                    }

                    await using var blobStream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
                    using var reader = new StreamReader(blobStream);
                    var jsonInString = await reader.ReadToEndAsync();
                    var jMessages = JArray.Parse(jsonInString);

                    logger.LogInformation("Total data of the message : {Total}", jMessages.Count);

                    foreach (var t in jMessages)
                        await FunctionSyncUpdater<T>.UpdateDatabaseAsync(t, dbContext, logger, cancellationToken);

                    await dbContext.SaveRefChangesAsync(cancellationToken);

                    item.IsFixed = true;
                    item.FixedDt = DateTime.UtcNow;
                    item.FixedMessage = $"Data successfully updated after retry {item.ErrorCount + 1} of 3";
                    await syncTableRepository.UpdateAsync(item.Id, item);
                }
                catch (Exception ex)
                {
                    item.ErrorCount++;

                    if (item.ErrorCount >= _maxRetry)
                    {
                        item.IsFixed = true;
                        item.FixedDt = DateTime.UtcNow;
                        item.FixedMessage = "Item is excluded, already exceed maximum retries";
                        item.IsFixedBecauseOfExceedRetry = true;
                    }

                    await syncTableRepository.UpdateAsync(item.Id, item);
                    await syncTableHistoryRepository.CreateAsync(new SyncTableHistory
                    {
                        SyncTableId = item.Id,
                        Message = ex.Message,
                        InnerMessage = ex.InnerException?.Message,
                        StackTrace = ex.StackTrace
                    });
                }

                if (item.ErrorCount < _maxRetry)
                    continue;

                var histories = await syncTableHistoryRepository.GetAsync(e => e.SyncTableId == item.Id);

                var failRecipients = configuration.GetSection("SyncRefTable:FailRecipients").Get<string[]>();
                await FunctionSyncEmailRetryFailed.SendEmailRetryFailedAsync(scope,
                    item,
                    histories.Items,
                    failRecipients, logger);
            }
        }
    }
}
