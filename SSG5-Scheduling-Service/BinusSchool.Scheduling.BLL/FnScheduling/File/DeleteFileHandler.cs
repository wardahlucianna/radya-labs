using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Scheduling.FnSchedule.File.Validator;
using Microsoft.WindowsAzure.Storage;

namespace BinusSchool.Scheduling.FnSchedule.File
{
    public class DeleteFileHandler : FunctionsHttpSingleHandler
    {
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();
            var body = await Request.ValidateBody<FileRequest, DeleteFileValidator>();
            var containerBlobName = body.Container;

            if (!KeyValues.TryGetValue(nameof(ExecutionContext), out var context))
                throw new System.ArgumentNullException("ExecutionContext not provided");
            var executionContext = context as ExecutionContext;

            await RemoveFile(body.BlobName, executionContext, containerBlobName);

            return Request.CreateApiResult2();
        }

        public Task RemoveFile(string fileName, ExecutionContext context, string containerName)
        {
            var storageAccount = GetCloudStorageAccount(context);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            var blockBlob = container.GetBlockBlobReference(fileName);

            return blockBlob.DeleteIfExistsAsync();
        }

        public CloudStorageAccount GetCloudStorageAccount(ExecutionContext executionContext)
        {
            //var config = new ConfigurationBuilder()
            //                .SetBasePath(executionContext.FunctionAppDirectory)
            //                .AddJsonFile("local.settings.json", true, true)
            //                .AddEnvironmentVariables().Build();

            var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Scheduling:AccountStorage"]);
            return storageAccount;
        }
    }
}
