using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BinusSchool.Document.FnDocument.DocumentRequest.Helper
{
    public class PaymentEvidanceDocumentBlobHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;

        private const string _containerNameInit = "document-request-payment-evidance";

        public PaymentEvidanceDocumentBlobHelper(
            IMachineDateTime dateTime,
            IConfiguration configuration)
        {
            _dateTime = dateTime;
            _configuration = configuration;
        }

        private string BuildContainerName(string idStudent)
        {
            return _containerNameInit + "/" + idStudent;
        }

        public static string GetDocumentLink(string filename, string containerLink)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            string url = containerLink.Replace("?", "/" + filename + "?");

            return url;
        }

        public string GetContainerSasUri(string idStudent, int expiryHour, string storedPolicyName = null)
        {
            string containerInitName = _containerNameInit;

            string sasContainerToken;

            CloudBlobContainer container;

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference(containerInitName);

            // If no stored policy is specified, create a new access policy and define its constraints.
            if (storedPolicyName == null)
            {
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
                // to construct a shared access policy that is saved to the container's shared access policies.
                SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(expiryHour),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                // Generate the shared access signature on the container, setting the constraints directly on the signature.
                sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);
            }
            else
            {
                // Generate the shared access signature on the container. In this case, all of the constraints for the
                // shared access signature are specified on the stored access policy, which is provided by name.
                // It is also possible to specify some constraints on an ad hoc SAS and others on the stored access policy.
                sasContainerToken = container.GetSharedAccessSignature(null, storedPolicyName);
            }

            // Return the URI string for the container, including the SAS token.
            return container.Uri + sasContainerToken;

            //Return blob SAS Token
            //return sasContainerToken;
        }

        private string GetBlobSasUri(string idStudent, string blobName, int expiryHour, string policyName = null)
        {
            string containerInitName = _containerNameInit;

            string sasBlobToken;

            CloudBlobContainer container;


            //if (AppType == 1)
            //{
            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference(containerInitName);

            // Get a reference to a blob within the container.
            // Note that the blob may not exist yet, but a SAS can still be created for it.
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            if (policyName == null)
            {
                // Create a new access policy and define its constraints.
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
                // to construct a shared access policy that is saved to the container's shared access policies.
                SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(expiryHour),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                // Generate the shared access signature on the blob, setting the constraints directly on the signature.
                sasBlobToken = blob.GetSharedAccessSignature(adHocSAS);
            }
            else
            {
                // Generate the shared access signature on the blob. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                sasBlobToken = blob.GetSharedAccessSignature(null, policyName);
            }

            // Return the URI string for the container, including the SAS token.
            //return blob.Uri + sasBlobToken;

            //Return blob SAS Token
            return sasBlobToken;
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_configuration.GetConnectionString("Document:AccountStorage"));
                return storageAccount;
            }
            catch
            {
                //var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Document:AccountStorage"]);
                //return storageAccount;

                throw new BadRequestException("Failed to upload file");
            }

        }

        public void CreateContainerIfNotExists(string idStudent)
        {
            string containerName = BuildContainerName(idStudent);

            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var containers = new[] { containerName };

            foreach (var item in containers)
            {
                var blobContainer = blobClient.GetContainerReference(item);
                blobContainer.CreateIfNotExistsAsync();
            }
        }

        public async Task<UploadFileHelperResult> UploadFileAsync(string idStudent, IFormFile file)
        {
            try
            {
                CloudBlockBlob blob;
                string uploadedFileName;
                string containerName = BuildContainerName(idStudent);

                string extension = Path.GetExtension(file.FileName);
                string originalFileName = Path.GetFileNameWithoutExtension(file.FileName);

                #region Upload File to Azure Storage  
                // create file container in blob storage 
                CreateContainerIfNotExists(idStudent);

                CloudStorageAccount storageAccount = GetCloudStorageAccount();
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                uploadedFileName = originalFileName + "_" + _dateTime.ServerTime.ToString("yyyyHHmmss") + extension;

                blob = container.GetBlockBlobReference(uploadedFileName);
                blob.Properties.ContentType = file.ContentType;

                //upload to azure storage
                await blob.UploadFromStreamAsync(file.OpenReadStream());
                #endregion

                var result = new UploadFileHelperResult
                {
                    UploadedFileName = uploadedFileName,
                    UploadedFileUrl = blob.Uri.AbsoluteUri
                };

                return result;
            }
            catch (Exception)
            {
                throw new BadRequestException("Failed to upload file");
            }
            
        }

        public Task RemoveFileIfExists(string idStudent, string fileName)
        {
            string containerName = BuildContainerName(idStudent);

            if (string.IsNullOrEmpty(fileName))
                return Task.CompletedTask;

            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);

            return blockBlob.DeleteIfExistsAsync();
        }
    }
}
