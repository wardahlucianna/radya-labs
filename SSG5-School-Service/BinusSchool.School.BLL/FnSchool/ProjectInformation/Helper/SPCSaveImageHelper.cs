using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BinusSchool.School.FnSchool.ProjectInformation.Helper
{
    public class SPCSaveImageHelper
    {
        private readonly IConfiguration _configuration;
        private const string _containerNameInit = "school-project-coordinator";

        public SPCSaveImageHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_configuration.GetConnectionString("School:AccountStorage"));
                return storageAccount;
            }
            catch
            {
                throw new BadRequestException("Failed to Upload File");
            }
        }

        public async Task<IEnumerable<string>> MoveFilesAndDeleteFromSourceContainerAsync(string Url, string sourceContainerName, string idBinusian, string fileType)
        {
            // Get the source container reference
            CloudStorageAccount sourceStorageAccount = GetCloudStorageAccount();
            CloudBlobClient sourceBlobClient = sourceStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer sourceContainer = sourceBlobClient.GetContainerReference(sourceContainerName);

            // Get the destination container reference
            CloudStorageAccount destinationStorageAccount = GetCloudStorageAccount();
            CloudBlobClient destinationBlobClient = destinationStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer destinationContainer = destinationBlobClient.GetContainerReference(_containerNameInit);

            // Get the temporary blob reference
            CloudBlockBlob sourceBlob = sourceContainer.GetBlockBlobReference(Url);

            // Check if the source blob exists
            if (!await sourceBlob.ExistsAsync())
            {
                throw new ArgumentException($"The specified blob URL '{sourceBlob.Uri.AbsoluteUri}' does not exist.", nameof(sourceBlob));
            }

            // Set the destination blob name with the new filename format
            string destinationBlobName = idBinusian + "." + fileType;

            // Check if a file with the same name already exists in the destination container
            CloudBlockBlob existingBlob = destinationContainer.GetBlockBlobReference("school-project-coordinator/" + idBinusian);
            if (await existingBlob.ExistsAsync())
            {
                await existingBlob.DeleteAsync();
            }

            // Copy the file to the destination container
            CloudBlobDirectory destinationDirectory = destinationContainer.GetDirectoryReference("");
            CloudBlockBlob destinationBlob = destinationDirectory.GetBlockBlobReference(destinationBlobName);
            await destinationBlob.StartCopyAsync(sourceBlob);

            // Wait for the copy operation to complete
            while (true)
            {
                await destinationBlob.FetchAttributesAsync();
                if (destinationBlob.CopyState.Status != CopyStatus.Pending)
                {
                    break;
                }
                await Task.Delay(1000);
            }

            if (destinationBlob.CopyState.Status != CopyStatus.Success)
            {
                throw new Exception($"Failed to copy the file from source blob URL '{sourceBlob.Uri.AbsoluteUri}' to the destination container.");
            }

            // Delete the file from the source container
            await sourceBlob.DeleteAsync();

            // Add the URL of the moved file to the result list
            var destinationBlobUrls = new List<string> { destinationBlob.Uri.AbsoluteUri };

            return destinationBlobUrls;
        }

        //Delete logic for save and delete
        public async Task RemoveFileIfExists(string fileName, string photoLink)
        {
            if (string.IsNullOrEmpty(fileName))
                return;
            string[] paths = photoLink.Split('.');
            string fileType = paths[paths.Length - 1];

            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_containerNameInit);
            var fileBlob = container.GetBlobReference(fileName + "." + fileType);

            await fileBlob.DeleteIfExistsAsync();

        }
        //Delete logic for cancellation
        public async Task RemoveFileWithFileName(string fileName)
        {
            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_containerNameInit);
            var fileBlob = container.GetBlobReference(fileName);

            await fileBlob.DeleteIfExistsAsync();

        }
    }
}
