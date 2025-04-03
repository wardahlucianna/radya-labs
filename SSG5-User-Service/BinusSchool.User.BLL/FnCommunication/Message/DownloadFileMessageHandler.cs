using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Drawing;

namespace BinusSchool.User.FnCommunication.Message
{
    public class DownloadFileMessageHandler : FunctionsHttpSingleHandler
    {
        private readonly IStorageManager _storageManager;

        public DownloadFileMessageHandler(IStorageManager storageManager)
        {
            _storageManager = storageManager;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<DownloadFileMessageRequest>(nameof(DownloadFileMessageRequest.FileName));

            var blobContainer = await _storageManager.GetOrCreateBlobContainer("communication-file", PublicAccessType.None, CancellationToken);
            var blob = blobContainer.GetBlobClient(param.FileName);
            
            if (!(await blob.ExistsAsync(CancellationToken)).Value)
                throw new BadRequestException("Blob not found");

            using var str = new MemoryStream();
            await blob.DownloadToAsync(str, CancellationToken);
            var blobMetadata = await blob.GetPropertiesAsync(cancellationToken: CancellationToken);

            var fileName = param.OriginFileName;
            var contentType = blobMetadata.Value.ContentType;
            return new FileContentResult(str.ToArray(), contentType)
            {
                FileDownloadName = fileName
            };
        }
    }
}