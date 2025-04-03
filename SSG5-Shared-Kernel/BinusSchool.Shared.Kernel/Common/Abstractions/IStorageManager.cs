using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BinusSchool.Common.Abstractions
{
    public interface IStorageManager
    {
        string AccountName { get; }
        
        Task<(BlobClient source, BlobClient destination)> CopyBlob(BlobContainerClient blobContainer, string sourceFileName, string destFileName, CancellationToken ct = default);
        Task<BlobContainerClient> GetOrCreateBlobContainer(string containerName, PublicAccessType publicAccess = PublicAccessType.None, CancellationToken ct = default);
    }

    public interface IStorageManager<T> : IStorageManager {}
}
