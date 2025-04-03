using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.File;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BinusSchool.Document.FnDocument.File
{
    public class DeleteFileHandler : FunctionsHttpSingleHandler
    {
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();
            var containerBlobName = Configuration["PathName"];
            var param = Request.ValidateParams<FileRequest>(nameof(FileRequest.FileName));

            if (!KeyValues.TryGetValue(nameof(ExecutionContext), out var context))
                throw new System.ArgumentNullException("ExecutionContext not provided");
            var executionContext = context as ExecutionContext;

            await RemoveFile(param.FileName, executionContext, containerBlobName);

            return Request.CreateApiResult2();
        }

        public void CreateContainerIfNotExists(string containerName, ExecutionContext executionContext)
        {
            var storageAccount = GetCloudStorageAccount(executionContext);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var containers = new[] { containerName };

            foreach (var item in containers)
            {
                var blobContainer = blobClient.GetContainerReference(item);
                blobContainer.CreateIfNotExistsAsync();
            }
        }

        public Task RemoveFile(string fileName, ExecutionContext context, string containerName) 
        {
            var storageAccount = GetCloudStorageAccount(context);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);

            return blockBlob.DeleteIfExistsAsync();
        }

        #region Private Method

        public CloudStorageAccount GetCloudStorageAccount(ExecutionContext executionContext)
        {
            //var config = new ConfigurationBuilder()
            //                .SetBasePath(executionContext.FunctionAppDirectory)
            //                .AddJsonFile("local.settings.json", true, true)
            //                .AddEnvironmentVariables().Build();

            var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Document:AccountStorage"]);
            return storageAccount;
        }

        #endregion
    }
}