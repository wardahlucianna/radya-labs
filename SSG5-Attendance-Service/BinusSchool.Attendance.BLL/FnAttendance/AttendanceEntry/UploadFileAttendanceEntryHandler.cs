using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using ByteSizeLib;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry
{
    public class UploadFileAttendanceEntryHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _allowedFiles = new[] { ".png", ".jpg", ".jpeg", ".doc", ".docx", ".xls", ".xlsx", ".pdf" };
        // private static readonly ByteSize _maxFileSize = ByteSize.FromMegaBytes(5);

        private readonly IAttendanceDbContext _dbContext;
        private readonly IStorageManager _storageManager;

        public UploadFileAttendanceEntryHandler(IAttendanceDbContext dbContext, IStorageManager storageManager)
        {
            _dbContext = dbContext;
            _storageManager = storageManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<UploadFileAttendanceEntryRequest>(nameof(UploadFileAttendanceEntryRequest.IdGeneratedScheduleLesson));

            var file = Request.Form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
                throw new BadRequestException("Attendance file not provided");
            
            var fileInfo = new FileInfo(file.FileName);
            if (!_allowedFiles.Any(x => x == fileInfo.Extension))
                throw new BadRequestException($"File not allowed. Allowed file: {string.Join(", ", _allowedFiles)}");
            // if (fileInfo.Length > _maxFileSize.Bytes)
            //     throw new BadRequestException($"File too large. Max file size is {_maxFileSize.ToString("MB")}");

            var schedule = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Select(x => new { x.ScheduleDate, x.GeneratedScheduleStudent.IdStudent, x.Id, x.IdSession})
                .FirstOrDefaultAsync(CancellationToken);
            if (schedule is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Attendance"], "Id", param.IdGeneratedScheduleLesson));

            var blobName = BlobUtil.CreateBlobNameAttendance(schedule.ScheduleDate, schedule.IdStudent, schedule.Id);
            var blobMetadata = BlobUtil.ToAttendanceEntryMetadata(AuthInfo.UserId, schedule.IdSession, file.FileName, file.ContentType);
            
            var blobContainer = await _storageManager.GetOrCreateBlobContainer("attendance-entry", PublicAccessType.None, CancellationToken);
            var blob = blobContainer.GetBlobClient(blobName);

            using var fs = file.OpenReadStream();
            await blob.UploadAsync(fs, metadata: blobMetadata, cancellationToken: CancellationToken);

            return Request.CreateApiResult2(blobName as object);
        }
    }
}
