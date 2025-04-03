using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageDeleteFileAttachmentHandler : FunctionsHttpSingleHandler
    {

        private readonly IUserDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public MessageDeleteFileAttachmentHandler(
            IUserDbContext dbContext,
            IConfiguration configuration
            )
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();
            var containerBlobName = Configuration["PathName"];
            var param = Request.ValidateParams<GetFileRequest>(nameof(GetFileRequest.FileName));

            if (!KeyValues.TryGetValue(nameof(ExecutionContext), out var context))
                throw new System.ArgumentNullException("ExecutionContext not provided");
            var executionContext = context as ExecutionContext;

            await RemoveFile(param.FileName, executionContext, containerBlobName);

            var data = await _dbContext.Entity<TrMessageAttachment>()
                                       .Where(x => x.Id == param.IdMessageAttachment && x.IsActive == true)
                                       .SingleOrDefaultAsync();
            if (data is null)
                throw new NotFoundException("File is not found");

            data.IsActive = false;
            _dbContext.Entity<TrMessageAttachment>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);

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

            var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:User:AccountStorage"]);
            return storageAccount;
        }

        #endregion
    }
}