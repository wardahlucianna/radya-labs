using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Table.Entities;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Storage;
using BinusSchool.Domain.Abstractions;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BinusSchool.Common.Functions.Handler
{
    public abstract class FunctionsSyncRefTableHandler2<T> where T : DbContext, IAppDbContext
    {
        protected const string Route = "retry-sync";
        protected const string Tag = "Retry Synchronize Reference Table";

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

        private bool _messageInBlob;
        private readonly string _hubName;

        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly T _dbContext;
        private readonly ILogger<FunctionsSyncRefTableHandler2<T>> _logger;

        protected FunctionsSyncRefTableHandler2(IServiceProvider services)
        {
            _services = services;
            _configuration = services.GetService<IConfiguration>();
            _dbContext = services.GetService<T>();
            _logger = services.GetService<ILogger<FunctionsSyncRefTableHandler2<T>>>();
        }

        protected FunctionsSyncRefTableHandler2(IServiceProvider services, string hubName) : this(services)
        {
            _hubName = hubName;
        }

        protected async Task Synchronize(string message, Guid invocationId, CancellationToken cancellationToken)
        {
            var jMessages = default(JArray);
            var blobClient = default(BlobClient);
            var needToDeleteBlob = false;

            _logger.LogInformation("Sync data started");

            try
            {
                jMessages = JArray.Parse(message);
                if (jMessages.Count == 0)
                {
                    _logger.LogInformation("0 data sent, automatically returned");
                    return;
                }

                _logger.LogInformation("Total data sent : {Total}", jMessages.Count);

                var rowCount = 0;

                if (jMessages[0].Value<bool>(nameof(MessageInBlob.StoredInBlob)))
                {
                    _messageInBlob = true;
                    _logger.LogInformation("Message are stored in the Blob storage, instead of EventHub");

                    // NOTE: changes are stored in blob, need to fetch it first
                    var auditStorageConnString = _configuration.GetSection("ConnectionStrings:Audit:AccountStorage")
                        .Get<string>();
                    var storageManager = new StorageManager(auditStorageConnString,
                        _services.GetService<ILogger<StorageManager>>());

                    var blobName = jMessages[0].Value<string>(nameof(MessageInBlob.BlobName));
                    var blobContainerName = jMessages[0].Value<string>(nameof(MessageInBlob.BlobContainer));
                    _logger.LogInformation("[Blob] Reading blob {BlobName} from {BlobContainerName} container",
                        blobName, blobContainerName);

                    var blobContainer =
                        await storageManager.GetOrCreateBlobContainer(blobContainerName, ct: cancellationToken);
                    blobClient = blobContainer.GetBlobClient(blobName);

                    await using var blobStream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
                    using var blobStreamReader = new StreamReader(blobStream);
                    using var jsonReader = new JsonTextReader(blobStreamReader);

                    var entry = 0;
                    while (await jsonReader.ReadAsync(cancellationToken))
                    {
                        if (jsonReader.TokenType != JsonToken.StartObject) continue;

                        var syncRefTable = await JToken.ReadFromAsync(jsonReader, cancellationToken);

                        UpdateDatabase(syncRefTable);

                        try
                        {
                            await _dbContext.SaveRefChangesAsync(cancellationToken);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Error occured while saving data from JSON entry {Entry}", entry);
                        }

                        entry++;
                        rowCount++;
                    }

                    // if operation above success, delete blob process will be executed after success save to db
                    needToDeleteBlob = true;
                }
                else
                {
                    _logger.LogInformation("Message are stored in EventHub");

                    var entry = 0;
                    foreach (var jMessage in jMessages)
                    {
                        UpdateDatabase(jMessage);

                        entry++;
                        rowCount++;
                    }

                    try
                    {
                        await _dbContext.SaveRefChangesAsync(cancellationToken);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error occured while saving data from JSON entry {Entry}", entry);
                    }
                }

                _logger.LogInformation("Row Count: {RowCount}", rowCount);
            }
            catch (Exception ex)
            {
                if (ex is RequestFailedException reqEx)
                    _logger.LogError(reqEx, "[Blob] {Message}", reqEx.Message);
                else
                    _logger.LogError(ex, "[EventHubs] {Message}", ex.Message);

                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    if (ex.InnerException.Message.Contains("Cannot insert duplicate key in object"))
                        throw;
                }

                needToDeleteBlob = false; // make sure to revoke delete blob process

                // collect table name
                var affectedTables = Array.Empty<string>();
                if (_messageInBlob)
                    affectedTables = new[] { "JSON" };
                else if (jMessages != null && jMessages.Count != 0)
                    affectedTables = jMessages
                        .Select(x => x.Value<string>(nameof(SyncRefTable.Table)))
                        .Where(x => x != null).Distinct().ToArray();

                var tableManager = _services.GetService<ITableManager>();
                var failSync = new HsFailSyncRefTable
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    PartitionKey = _hubName,
                    RowKey = Guid.NewGuid().ToString(),
                    Message = ex.Message,
                    InnerMessage = ex.InnerException?.Message,
                    AffectTable = affectedTables.Length != 0 ? string.Join(",", affectedTables) : null,
                    Value = message,
                    InvocationId = invocationId,
                    OperationId = System.Diagnostics.Activity.Current?.RootId
                };

                await tableManager.InsertAndSave(failSync, cancellationToken);

                var failRecipients = _configuration.GetSection("SyncRefTable:FailRecipients").Get<string[]>();
                if (failRecipients != null && failRecipients.Length != 0)
                {
                    var notificationManager = _services.GetRequiredService<INotificationManager>();
                    var currentFunctions = _services.GetRequiredService<ICurrentFunctions>();

                    var data = new Dictionary<string, object>
                    {
                        { "nl", Environment.NewLine },
                        { "pk", failSync.PartitionKey },
                        { "rk", failSync.RowKey },
                        { "iid", failSync.InvocationId },
                        { "oid", failSync.OperationId },
                        { "msg", failSync.Message },
                        { "inmsg", failSync.InnerMessage },
                        { "afftd", failSync.AffectTable },
                        { "val", JsonConvert.DeserializeObject(failSync.Value) },
                        { "mind", failSync.Timestamp.AddDays(-1).ToString("yyyy-MM-dd") },
                        { "maxd", failSync.Timestamp.AddDays(1).ToString("yyyy-MM-dd") },
                    };
                    var mailTemplate = Handlebars.Compile(_failBodyFormat);
                    var mailMessage = new EmailData
                    {
                        ToAddresses = failRecipients.Select(x => new Address(x)).ToList(),
                        Subject =
                            $"{Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT")} - Fail to sync {_hubName} in {currentFunctions.Domain} service",
                        Body = mailTemplate(data),
                        Tags = new List<string>(new[] { "SyncRefTable", "PubSub" })
                    };
                    await notificationManager.SendSmtp(mailMessage);
                }

                throw;
            }
            finally
            {
                //delete the blob here
                if (blobClient != null && needToDeleteBlob)
                {
                    var deleteResult = await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots,
                        cancellationToken: cancellationToken);
                    _logger.LogInformation("[Blob] Deleted ({Status}) blob {Name} in {BlobContainerName} container",
                        deleteResult.Status,
                        blobClient.Name, blobClient.BlobContainerName);
                }
            }
        }

        private void UpdateDatabase(JToken syncRefTable)
        {
            var id = syncRefTable[nameof(SyncRefTable.Id)]?.ToObject<IDictionary<string, string>>() ??
                     new Dictionary<string, string>();
            var action = syncRefTable.Value<int>(nameof(SyncRefTable.Action));
            var jValue = syncRefTable[nameof(SyncRefTable.Value)];

            // define & assign field identity from single or composite PK of entity
            foreach (var pk in id)
            {
                jValue[pk.Key] = pk.Value;
            }

            var entityName = syncRefTable.Value<string>(nameof(SyncRefTable.Table));

            _logger.LogInformation("Given entity type: {FullName}", entityName);

            var entityType =
                typeof(T).Assembly.ExportedTypes.FirstOrDefault(x =>
                    x.Name == entityName && typeof(IEntity).IsAssignableFrom(x));
            if (entityType is null)
                return;

            var entityInstance = JsonConvert.DeserializeObject(jValue.ToString(), entityType);
            if (entityInstance is null)
                return;

            _logger.LogInformation("Using entity type: {FullName}", entityType.FullName);
            var existingEntry = _dbContext.Find(entityType, id.Select(x => (object)x.Value).ToArray());

            var auditAction = (AuditAction)action;
            if (auditAction == AuditAction.Insert || auditAction == AuditAction.Update)
            {
                if (existingEntry is null)
                {
                    _dbContext.Add(entityInstance);
                }
                else
                {
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

                        // _logger.LogInformation("[Assigning] {0} = {1} ({2})", prop.Name, prop.Value.ToString(), propDestination.PropertyType);
                        _dbContext.Entry(existingEntry).Property(prop.Name).CurrentValue =
                            prop.Value.ToObject(propDestination.PropertyType);
                    }

                    _dbContext.Update(existingEntry);
                }
            }
            else if (auditAction == AuditAction.Remove && existingEntry is { })
            {
                _dbContext.Remove(existingEntry);
            }
        }

        public class RetrySyncRequest
        {
            public string HubName { get; set; }
            public string RowKey { get; set; }
        }

        protected async Task<IActionResult> RetrySynchronize(HttpRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var baseTable = request.ValidateParams<RetrySyncRequest>(nameof(RetrySyncRequest.HubName),
                    nameof(RetrySyncRequest.RowKey));

                _logger.LogInformation("[EventHubs] Retry sync on Hub {HubName} with RowKey {RowKey}",
                    baseTable.HubName,
                    baseTable.RowKey);

                var tableManager = _services.GetService<ITableManager>();
                var table = tableManager.TableClient.GetTableReference(nameof(HsFailSyncRefTable));

                var queryResult = await table.CreateQuery<HsFailSyncRefTable>()
                    .Where(x => x.PartitionKey == baseTable.HubName && x.RowKey == baseTable.RowKey)
                    .AsTableQuery()
                    .ExecuteSegmentedAsync(null, cancellationToken);
                var logFailed = queryResult.FirstOrDefault();
                if (logFailed is null)
                    throw new NotFoundException(
                        $"Item in Table {nameof(HsFailSyncRefTable)} with PK: {baseTable.HubName} & RK: {baseTable.RowKey} is not found.");

                // retry here
                await Synchronize(logFailed.Value, Guid.Empty, cancellationToken);

                // delete log failed sync when retry successful
                var deleteOperation = TableOperation.Delete(logFailed);
                await table.ExecuteAsync(deleteOperation, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EventHubs] {Message}", ex.Message);
                return request.CreateApiErrorResponse(ex);
            }

            return request.CreateApiResponse(null);
        }
    }
}
