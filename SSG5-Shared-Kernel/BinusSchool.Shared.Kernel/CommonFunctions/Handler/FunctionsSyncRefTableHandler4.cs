using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Repositories;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Storage;
using BinusSchool.Common.Utils;
using BinusSchool.Domain.Abstractions;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BinusSchool.Common.Functions.Handler
{
    public class FunctionsSyncRefTableHandler4<T> where T : DbContext, IAppDbContext
    {
        private const string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        private const string _failBodyFormat = @"
            Hub name        : {{pk}}
            Row key         : {{rk}}
            Invocation id   : {{iid}}
            Operation id    : {{oid}}
            Message         : {{msg}}
            Inner message   : {{inmsg}}
            Affect tables   : {{afftd}}
            Value           : {{val}}

            Application insights query:
            union traces
                | union exceptions
                | where timestamp between (datetime(""{{mind}}"") .. datetime(""{{maxd}}""))
                | where operation_Id == '{{oid}}'
                | where customDimensions['InvocationId'] == '{{iid}}'
                | order by timestamp asc
                | project
                    timestamp,
                    message = iff(message != '', message, iff(innermostMessage != '', innermostMessage, customDimensions.['prop__{OriginalFormat}'])),
                    logLevel = customDimensions.['LogLevel']
        ";

        private const string _failRetryExceedBodyFormat = @"
            Retry already exceed 3 times, with details below (DateTime are in UTC+0):

            Id : {{Id}}
            Domain : {{Domain}}
            FileName : {{FileName}}
            Initial Error Msg : {{Message}}
            First Retry Msg : {{FirstTryMessage}}
            First Retry On : {{FirstTryOn}}
            Second Retry Msg : {{SecondTryMessage}}
            Second Retry On : {{SecondTryOn}}
            Third Retry Msg : {{ThirdTryMessage}}
            Third Retry On : {{ThirdTryOn}}
        ";

        private const int _maxRetry = 3;
        private const int _take = 2;

        private bool _messageInBlob;
        private readonly string _hubName;
        private readonly string _domain;

        private readonly IServiceProvider _services;

        protected FunctionsSyncRefTableHandler4(IServiceProvider services, string hubName, string domain)
        {
            _services = services;
            _hubName = hubName;
            _domain = domain;
        }

        protected async Task Synchronize(string message, Guid invocationId, CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            await ExecuteAsync(message, invocationId, scope, cancellationToken);
        }

        protected async Task RetrySyncAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            await ExecuteRetrySyncAsync(scope, cancellationToken);
        }

        private async Task ExecuteRetrySyncAsync(
            IServiceScope scope,
            CancellationToken cancellationToken)
        {
            var dbContext = scope.ServiceProvider.GetService<T>();
            var logger = scope.ServiceProvider.GetService<ILogger<FunctionsSyncRefTableHandler4<T>>>();
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
                        await UpdateDatabaseAsync(t, dbContext, logger, cancellationToken);

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
                await SendEmailRetryFailedAsync(scope, item, histories.Items, failRecipients, logger);
            }
        }

        private async Task SendEmailRetryFailedAsync(IServiceScope scope,
            SyncTable item,
            IEnumerable<SyncTableHistory> historiesItems,
            IReadOnlyCollection<string> failRecipients, ILogger<FunctionsSyncRefTableHandler4<T>> logger)
        {
            if (failRecipients == null || failRecipients.Count == 0)
                return;

            //only for production or staging
            var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env) || env.ToUpper() == "DEVELOPMENT")
                return;

            var notificationManager = scope.ServiceProvider.GetRequiredService<INotificationManager>();
            var currentFunctions = scope.ServiceProvider.GetRequiredService<ICurrentFunctions>();

            var mailTemplate = Handlebars.Compile(_failRetryExceedBodyFormat);
            var firstTypeMessage = string.Empty;
            var firstTypeOn = string.Empty;
            var secondTypeMessage = string.Empty;
            var secondTypeOn = string.Empty;
            var thirdTypeMessage = string.Empty;
            var thirdTypeOn = string.Empty;

            var syncTableHistories = historiesItems.ToList();
            if (syncTableHistories.Any())
                for (var i = 0; i < syncTableHistories.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            firstTypeMessage = syncTableHistories[i].Message;
                            firstTypeOn = syncTableHistories[i].CreatedDate.ToString(_dateTimeFormat);
                            break;
                        case 1:
                            secondTypeMessage = syncTableHistories[i].Message;
                            secondTypeOn = syncTableHistories[i].CreatedDate.ToString(_dateTimeFormat);
                            break;
                        case 2:
                            thirdTypeMessage = syncTableHistories[i].Message;
                            thirdTypeOn = syncTableHistories[i].CreatedDate.ToString(_dateTimeFormat);
                            break;
                    }
                }

            var data = new Dictionary<string, object>
            {
                { "Id", item.Id },
                { "Domain", item.Domain },
                { "FileName", item.Filename },
                { "Message", item.Message },
                { "FirstTryMessage", firstTypeMessage },
                { "FirstTryOn", firstTypeOn },
                { "SecondTryMessage", secondTypeMessage },
                { "SecondTryOn", secondTypeOn },
                { "ThirdTryMessage", thirdTypeMessage },
                { "ThirdTryOn", thirdTypeOn }
            };
            var subject =
                $"{env} - Retry exceed maximum of ID {item.Id} in {currentFunctions.Domain} service";
            var mailMessage = new EmailData
            {
                ToAddresses = failRecipients.Select(x => new Address(x)).ToList(),
                Subject = subject,
                Body = mailTemplate(data),
                Tags = new List<string>(new[] { "SyncRefTable", "PubSub" })
            };

            await notificationManager.SendSmtp(mailMessage);

            logger.LogInformation("Send email failed retries pub sub is successfully send to fail recipients");
        }

        private async Task ExecuteAsync(string message,
            Guid invocationId,
            IServiceScope scope,
            CancellationToken cancellationToken)
        {
            var dbContext = scope.ServiceProvider.GetService<T>();
            var logger = scope.ServiceProvider.GetService<ILogger<FunctionsSyncRefTableHandler4<T>>>();
            var configuration = scope.ServiceProvider.GetService<IConfiguration>();
            var blobAccountName = string.Empty;
            var blobName = string.Empty;
            var blobContainerName = string.Empty;
            var auditStorageConnString = configuration.GetSection("ConnectionStrings:Audit:AccountStorage")
                .Get<string>();
            var storageManager = new StorageManager(auditStorageConnString,
                scope.ServiceProvider.GetService<ILogger<StorageManager>>());

            logger.LogInformation("Event sync ref to {HubName}", _hubName);

            var jMessages = JArray.Parse(message);
            if (jMessages.Count == 0)
            {
                logger.LogInformation("0 data sent, automatically returned");
                return;
            }

            logger.LogInformation("Total data sent : {Total}", jMessages.Count);

            try
            {
                if (jMessages[0].Value<bool>(nameof(MessageInBlob.StoredInBlob)))
                {
                    _messageInBlob = true;

                    logger.LogInformation("Message are stored in the Blob storage");

                    blobAccountName = storageManager.AccountName;
                    blobName = jMessages[0].Value<string>(nameof(MessageInBlob.BlobName));
                    blobContainerName = jMessages[0].Value<string>(nameof(MessageInBlob.BlobContainer));

                    logger.LogInformation("Reading blob {BlobName} from {BlobContainerName} container",
                        blobName, blobContainerName);

                    var blobContainer =
                        await storageManager.GetOrCreateBlobContainer(blobContainerName, ct: cancellationToken);
                    var blobClient = blobContainer.GetBlobClient(blobName);

                    await using var blobStream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
                    using var reader = new StreamReader(blobStream);
                    var jsonInString = await reader.ReadToEndAsync();

                    jMessages = JArray.Parse(jsonInString);

                    if (jMessages.Count == 0)
                        logger.LogInformation("Data json from blob total 0 data sent, automatically returned");
                }
                else
                    logger.LogInformation("Message are stored in EventHub");

                if (jMessages.Count == 0)
                    return;

                for (var i = 0; i < jMessages.Count; i++)
                    await UpdateDatabaseAsync(jMessages[i], dbContext, logger, cancellationToken);

                await dbContext.SaveRefChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex is RequestFailedException reqEx)
                    logger.LogError(reqEx, "[Blob] {Message}", reqEx.Message);
                else
                    logger.LogError(ex, "[EventHubs] {Message}", ex.Message);

                //when message stored in eventhub
                if (string.IsNullOrWhiteSpace(blobName))
                {
                    var now = DateTimeUtil.ServerTime;
                    blobName = $"{now:yyyy/MM/dd}/{Guid.NewGuid()}.json";
                    blobContainerName = "retry-sync-" + _hubName;
                    var blobContainer =
                        await storageManager.GetOrCreateBlobContainer(blobContainerName, ct: cancellationToken);
                    var changesBinary = new BinaryData(message);
                    // upload changes to blob
                    var blobResult = await blobContainer.UploadBlobAsync(blobName, changesBinary, cancellationToken);
                    var rawBlobResult = blobResult.GetRawResponse();
                    logger.LogInformation("[Blob] {0} blob {1} in {2} container.",
                        rawBlobResult.ReasonPhrase,
                        blobName,
                        blobContainerName);
                }

                var cosmosConnString = configuration.GetConnectionString("Sync:CosmosDbNoSql");
                var cosmosClient = new CosmosClient(cosmosConnString);
                var syncTableRepository = new CollectionOfRepositories.SyncTableRepository(cosmosClient);
                await syncTableRepository.CreateAsync(new SyncTable
                {
                    Source = _hubName,
                    Domain = _domain,
                    Storage = blobAccountName,
                    Container = blobContainerName,
                    Filename = blobName,
                    Message = ex.Message,
                    InnerMessage = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });

                // collect table name
                var affectedTables = Array.Empty<string>();
                if (_messageInBlob)
                    affectedTables = new[] { "JSON" };
                else if (jMessages.Count != 0)
                    affectedTables = jMessages
                        .Select(x => x.Value<string>(nameof(SyncRefTable.Table)))
                        .Where(x => x != null).Distinct().ToArray();

                var data = new Dictionary<string, object>
                {
                    { "nl", Environment.NewLine },
                    { "pk", _hubName },
                    { "rk", Guid.NewGuid().ToString() },
                    { "iid", invocationId },
                    { "oid", System.Diagnostics.Activity.Current?.RootId },
                    { "msg", ex.Message },
                    { "inmsg", ex.InnerException?.Message },
                    { "afftd", affectedTables.Length != 0 ? string.Join(",", affectedTables) : null },
                    { "val", JsonConvert.DeserializeObject(message) },
                    { "mind", DateTimeOffset.UtcNow.AddDays(-1).ToString(_dateTimeFormat) },
                    { "maxd", DateTimeOffset.UtcNow.AddDays(1).ToString(_dateTimeFormat) },
                };

                var failRecipients = configuration.GetSection("SyncRefTable:FailRecipients").Get<string[]>();
                await SendEmailAsync(scope, data, failRecipients);
            }
        }

        private async Task SendEmailAsync(IServiceScope scope,
            Dictionary<string, object> data,
            IReadOnlyCollection<string> failRecipients)
        {
            if (failRecipients == null || failRecipients.Count == 0)
                return;

            //only for production or staging
            var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env) || env.ToUpper() == "DEVELOPMENT")
                return;

            var notificationManager = scope.ServiceProvider.GetRequiredService<INotificationManager>();
            var currentFunctions = scope.ServiceProvider.GetRequiredService<ICurrentFunctions>();

            var mailTemplate = Handlebars.Compile(_failBodyFormat);
            var mailMessage = new EmailData
            {
                ToAddresses = failRecipients.Select(x => new Address(x)).ToList(),
                Subject =
                    $"{env} - Fail to sync {_hubName} in {currentFunctions.Domain} service",
                Body = mailTemplate(data),
                Tags = new List<string>(new[] { "SyncRefTable", "PubSub" })
            };

            await notificationManager.SendSmtp(mailMessage);
        }

        private async Task UpdateDatabaseAsync(JToken syncRefTable,
            T dbContext,
            ILogger<FunctionsSyncRefTableHandler4<T>> logger,
            CancellationToken cancellationToken)
        {
            var id = syncRefTable[nameof(SyncRefTable.Id)]?.ToObject<IDictionary<string, string>>() ??
                     new Dictionary<string, string>();
            var action = syncRefTable.Value<int>(nameof(SyncRefTable.Action));
            var jValue = syncRefTable[nameof(SyncRefTable.Value)];

            // define & assign field identity from single or composite PK of entity
            foreach (var pk in id)
                jValue[pk.Key] = pk.Value;

            var entityName = syncRefTable.Value<string>(nameof(SyncRefTable.Table));

            logger.LogInformation("Given entity type: {FullName}", entityName);

            var entityType =
                typeof(T).Assembly.ExportedTypes.FirstOrDefault(x =>
                    x.Name == entityName && typeof(IEntity).IsAssignableFrom(x));
            if (entityType is null)
                return;

            var entityInstance = JsonConvert.DeserializeObject(jValue.ToString(), entityType);
            if (entityInstance is null)
                return;

            var ids = id.Select(x => (object)x.Value).ToArray();
            var pkId = ids.First().ToString();

            logger.LogInformation("Using entity type: {FullName} of {Id}", entityType.FullName, pkId);

            var existingEntry = await dbContext.FindAsync(entityType,
                ids,
                cancellationToken);

            var auditAction = (AuditAction)action;
            if (auditAction == AuditAction.Insert || auditAction == AuditAction.Update)
            {
                if (existingEntry is null)
                {
                    logger.LogInformation("Entity is being added");
                    await dbContext.AddAsync(entityInstance, cancellationToken);
                }
                else
                {
                    logger.LogInformation("Entity is being updated");
                    var destProps = existingEntry.GetType()
                        .GetProperties()
                        .Where(x => x.CanWrite)
                        .Select(x => new { x.Name, x.PropertyType })
                        .ToList();
                    foreach (var jToken in jValue)
                    {
                        var prop = (JProperty)jToken;
                        var propDestination = destProps.Find(x => x.Name == prop.Name);

                        if (propDestination is null)
                            continue;

                        dbContext.Entry(existingEntry).Property(prop.Name).CurrentValue =
                            prop.Value.ToObject(propDestination.PropertyType);
                    }

                    dbContext.Update(existingEntry);
                }
            }

            if (auditAction == AuditAction.Remove && existingEntry is { })
                dbContext.Remove(existingEntry);
        }
    }
}
