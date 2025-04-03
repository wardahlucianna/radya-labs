using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BinusSchool.Student.FnStudent.MedicalSystem.Helper
{
    public class MedicalDocumentBlobHelper
    {
        private readonly IMachineDateTime _date;
        private readonly IConfiguration _configuration;

        private const string _container = "medical-document";

        public MedicalDocumentBlobHelper(IMachineDateTime date, IConfiguration configuration)
        {
            _date = date;
            _configuration = configuration;
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_configuration.GetConnectionString("Student:AccountStorage"));
                return storageAccount;
            }
            catch
            {
                var storageAccount = CloudStorageAccount.Parse(_configuration["ConnectionStrings:Student:AccountStorage"]);
                return storageAccount;
            }
        }

        public void CreateContainerIfNotExists()
        {
            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var containers = new[] { _container };

            foreach (var item in containers)
            {
                var blobContainer = blobClient.GetContainerReference(item);
                blobContainer.CreateIfNotExistsAsync();
            }
        }

        public async Task<UploadMedicalDocumentFileResult> UploadMedicalDocumentFile(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName);
            string uploadedFileName = Path.GetFileNameWithoutExtension(file.FileName);

            #region Upload File to Azure Storage
            CreateContainerIfNotExists();

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(_container);

            uploadedFileName = uploadedFileName + "_" + _date.ServerTime.ToString("yyyyHHmmss") + extension;

            CloudBlockBlob blob = container.GetBlockBlobReference(uploadedFileName);
            blob.Properties.ContentType = file.ContentType;

            await blob.UploadFromStreamAsync(file.OpenReadStream());
            #endregion

            return new UploadMedicalDocumentFileResult
            {
                FileName = uploadedFileName,
                FileUrl = blob.Uri.ToString(),
            };
        }

        public async Task<bool> RemoveFileIfExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_container);
            var blockBlob = container.GetBlockBlobReference(fileName);

            // Mengecek apakah file ada di storage
            bool exists = await blockBlob.ExistsAsync().ConfigureAwait(false);
            if (exists)
            {
                // Jika file ditemukan, hapus file
                await blockBlob.DeleteIfExistsAsync().ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public class UploadMedicalDocumentFileResult
        {
            public string FileName { get; set; }
            public string FileUrl { get; set; }
        }
    }
}
