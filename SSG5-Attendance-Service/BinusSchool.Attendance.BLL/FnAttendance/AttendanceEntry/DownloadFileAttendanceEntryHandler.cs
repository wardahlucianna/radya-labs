using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using BinusSchool.Attendance.FnAttendance.Utils;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using Microsoft.AspNetCore.Mvc;

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry
{
    public class DownloadFileAttendanceEntryHandler : FunctionsHttpSingleHandler
    {
        private readonly IStorageManager _storageManager;

        public DownloadFileAttendanceEntryHandler(IStorageManager storageManager)
        {
            _storageManager = storageManager;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<DownloadFileAttendanceEntryRequest>(nameof(DownloadFileAttendanceEntryRequest.FileName));

            var blobContainer = await _storageManager.GetOrCreateBlobContainer("attendance-entry", PublicAccessType.None, CancellationToken);
            var blob = blobContainer.GetBlobClient(param.FileName);
            
            if (!(await blob.ExistsAsync(CancellationToken)).Value)
                throw new BadRequestException("Blob not found");

            using var str = new MemoryStream();
            await blob.DownloadToAsync(str, CancellationToken);
            var blobMetadata = await blob.GetPropertiesAsync(cancellationToken: CancellationToken);
            var (userIn, idSession, fileName, contentType) = BlobUtil.GetAttendanceEntryMetadata(blobMetadata.Value.Metadata);

            return new FileContentResult(str.ToArray(), contentType)
            {
                FileDownloadName = fileName
            };
        }
    }
}