using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using BinusSchool.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Common.Storage
{
    public class StorageManager : IStorageManager
    {
        public string AccountName => _blobService.AccountName;
        
        private readonly BlobServiceClient _blobService;
        private readonly ILogger<StorageManager> _logger;

        public StorageManager(string connectionString, ILogger<StorageManager> logger)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            _blobService = new BlobServiceClient(connectionString);
            _logger = logger;
        }

        public async Task<(BlobClient source, BlobClient destination)> CopyBlob(BlobContainerClient blobContainer, string sourceFileName, string destFileName, CancellationToken ct = default)
        {
            var srcBlob = blobContainer.GetBlobClient(sourceFileName);
            if (!(await srcBlob.ExistsAsync(ct)).Value)
                throw new Exception("Blob not found");
            
            // acquire lease source blob to prevent other client modifying it 
            var lease = srcBlob.GetBlobLeaseClient();
            await lease.AcquireAsync(TimeSpan.FromMinutes(1), cancellationToken: ct);

            var destBlob = blobContainer.GetBlobClient(destFileName);
            await destBlob.StartCopyFromUriAsync(srcBlob.Uri, cancellationToken: ct);
            _logger.LogInformation($"[Blob] Copied blob: '{sourceFileName}' to '{destFileName}'");

            var sourceProp = await srcBlob.GetPropertiesAsync(cancellationToken: ct);
            if (sourceProp.Value.LeaseState == LeaseState.Leased)
            {
                // release lease after operation complete
                await lease.ReleaseAsync(cancellationToken: ct);
            }

            return (srcBlob, destBlob);
        }

        public async Task<BlobContainerClient> GetOrCreateBlobContainer(string containerName, PublicAccessType publicAccess = PublicAccessType.None, CancellationToken ct = default)
        {
            var blobContainer = _blobService.GetBlobContainerClient(containerName);
            var response = await blobContainer.CreateIfNotExistsAsync(publicAccess, cancellationToken: ct);
            var rawResponse = response?.GetRawResponse();
            
            if (rawResponse is {Status: (int)HttpStatusCode.Created})
                _logger.LogInformation($"[Blob] Container created: {containerName} ({blobContainer.Uri})");

            return blobContainer;
        }
    }

    public class StorageManager<T> : StorageManager, IStorageManager<T>
    {
        public StorageManager(string connectionString, ILogger<StorageManager> logger) : base(connectionString, logger)
        {
        }
    }
}
