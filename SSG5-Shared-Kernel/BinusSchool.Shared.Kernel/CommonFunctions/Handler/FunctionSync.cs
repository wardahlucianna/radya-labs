using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BinusSchool.Common.Functions.Repositories;
using BinusSchool.Common.Model;
using BinusSchool.Common.Storage;
using BinusSchool.Common.Utils;
using BinusSchool.Domain.Abstractions;
using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BinusSchool.Common.Functions.Handler
{
    public class FunctionSync<T> where T : DbContext, IAppDbContext
    {
        private const string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private readonly string _hubName;
        private readonly string _domain;
        private bool _messageInBlob;
        private T _dbContext;
        private ILogger<FunctionsSyncRefTableHandler4<T>> _logger;
        private IConfiguration _configuration;
        private StorageManager _storageManager;
        private JArray _jMessages;
        private IServiceScope _scope;
        private string _blobAccountName { get; set; }
        private string _blobName { get; set; }
        private bool _blobCanBeDeleted { get; set; }
        private string _blobContainerName { get; set; }
        private BlobClient _blobClient { get; set; }

        public FunctionSync(string hubName, string domain)
        {
            _hubName = hubName;
            _domain = domain;
            _blobCanBeDeleted = false;
        }

        public async Task RunAsync(string message,
            Guid invocationId,
            IServiceScope scope,
            CancellationToken cancellationToken)
        {
            _scope = scope;
            _dbContext = scope.ServiceProvider.GetService<T>();
            _logger = scope.ServiceProvider.GetService<ILogger<FunctionsSyncRefTableHandler4<T>>>();
            _configuration = scope.ServiceProvider.GetService<IConfiguration>();
            _blobAccountName = string.Empty;
            _blobName = string.Empty;
            _blobContainerName = string.Empty;
            _blobClient = null;

            var auditStorageConnString = _configuration.GetSection("ConnectionStrings:Audit:AccountStorage")
                .Get<string>();
            _storageManager = new StorageManager(auditStorageConnString,
                scope.ServiceProvider.GetService<ILogger<StorageManager>>());

            _logger.LogInformation("Event sync ref to {HubName}", _hubName);

            _jMessages = JArray.Parse(message);
            if (_jMessages.Count == 0)
            {
                _logger.LogInformation("0 data sent, automatically returned");
                return;
            }

            _logger.LogInformation("Total data sent : {Total}", _jMessages.Count);

            try
            {
                if (_jMessages[0].Value<bool>(nameof(MessageInBlob.StoredInBlob)))
                {
                    _messageInBlob = true;

                    _logger.LogInformation("Message are stored in the Blob storage");

                    _blobAccountName = _storageManager.AccountName;
                    _blobName = _jMessages[0].Value<string>(nameof(MessageInBlob.BlobName));
                    _blobContainerName = _jMessages[0].Value<string>(nameof(MessageInBlob.BlobContainer));

                    _logger.LogInformation("Reading blob {BlobName} from {BlobContainerName} container",
                        _blobName, _blobContainerName);

                    var blobContainer =
                        await _storageManager.GetOrCreateBlobContainer(_blobContainerName, ct: cancellationToken);
                    _blobClient = blobContainer.GetBlobClient(_blobName);

                    await using var blobStream = await _blobClient.OpenReadAsync(cancellationToken: cancellationToken);
                    using var reader = new StreamReader(blobStream);
                    var jsonInString = await reader.ReadToEndAsync();

                    _jMessages = JArray.Parse(jsonInString);

                    if (_jMessages.Count == 0)
                        _logger.LogInformation("Data json from blob total 0 data sent, automatically returned");
                }
                else
                    _logger.LogInformation("Message are stored in EventHub");

                if (_jMessages.Count == 0)
                    return;

                foreach (var t in _jMessages)
                    await FunctionSyncUpdater<T>.UpdateDatabaseAsync(t, _dbContext, _logger, cancellationToken);

                await _dbContext.SaveRefChangesAsync(cancellationToken);

                _blobCanBeDeleted = true;
            }
            catch (DbUpdateException dbUpdateException)
            {
                _logger.LogError(dbUpdateException, "[EventHubs] {Message}", dbUpdateException.Message);

                var byPass = true;
                if (dbUpdateException.InnerException != null && dbUpdateException.InnerException is SqlException)
                    if (dbUpdateException.InnerException.Message.StartsWith("Violation of PRIMARY KEY constraint") &&
                        _jMessages.Count > 1)
                    {
                        _logger.LogInformation(
                            "[EventHubs] Sync is bypass because already added to the database, and message total only 1");
                        byPass = false;
                    }

                if (!byPass)
                    await HandleErrorAsync(message, dbUpdateException, invocationId, cancellationToken);
            }
            catch (Exception ex)
            {
                if (ex is RequestFailedException reqEx)
                    _logger.LogError(reqEx, "[Blob] {Message}", reqEx.Message);
                else
                    _logger.LogError(ex, "[EventHubs] {Message}", ex.Message);

                await HandleErrorAsync(message, ex, invocationId, cancellationToken);
            }
            finally
            {
                if (_blobCanBeDeleted && _blobClient != null)
                {
                    var deleteResult =
                        await _blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots,
                            cancellationToken: cancellationToken);
                    _logger.LogInformation("[Blob] Deleted ({Status}) blob {Name} in {BlobContainerName} container",
                        deleteResult.Status,
                        _blobClient.Name,
                        _blobClient.BlobContainerName);
                }
            }
        }

        private async Task HandleErrorAsync(string message,
            Exception ex,
            Guid invocationId,
            CancellationToken cancellationToken)
        {
            //when message stored in eventhub
            if (string.IsNullOrWhiteSpace(_blobName))
            {
                var now = DateTimeUtil.ServerTime;
                _blobName = $"{now:yyyy/MM/dd}/{Guid.NewGuid()}.json";
                _blobContainerName = "sync-" + _hubName;
                var blobContainer =
                    await _storageManager.GetOrCreateBlobContainer(_blobContainerName, ct: cancellationToken);
                var changesBinary = new BinaryData(message);
                // upload changes to blob
                var blobResult = await blobContainer.UploadBlobAsync(_blobName, changesBinary, cancellationToken);
                var rawBlobResult = blobResult.GetRawResponse();
                _logger.LogInformation("[Blob] {0} blob {1} in {2} container.",
                    rawBlobResult.ReasonPhrase,
                    _blobName,
                    _blobContainerName);
            }

            var cosmosConnString = _configuration.GetConnectionString("Sync:CosmosDbNoSql");
            var cosmosClient = new CosmosClient(cosmosConnString);
            var syncTableRepository = new CollectionOfRepositories.SyncTableRepository(cosmosClient);
            await syncTableRepository.CreateAsync(new SyncTable
            {
                Source = _hubName,
                Domain = _domain,
                Storage = _blobAccountName,
                Container = _blobContainerName,
                Filename = _blobName,
                Message = ex.Message,
                InnerMessage = ex.InnerException?.Message,
                StackTrace = ex.StackTrace
            });

            // collect table name
            var affectedTables = Array.Empty<string>();
            if (_messageInBlob)
                affectedTables = new[] { "JSON" };
            else if (_jMessages.Count != 0)
                affectedTables = _jMessages
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

            var failRecipients = _configuration.GetSection("SyncRefTable:FailRecipients").Get<string[]>();
            await FunctionSyncEmail.SendEmailAsync(_scope, data, failRecipients, _hubName);
        }
    }
}
