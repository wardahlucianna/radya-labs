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
using BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry
{
    public class UploadFileEventAttendanceEntryHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _allowedFiles = new[] { ".png", ".jpg", ".jpeg", ".doc", ".docx", ".xls", ".xlsx", ".pdf" };

        private readonly IAttendanceDbContext _dbContext;
        private readonly IStorageManager _storageManager;

        public UploadFileEventAttendanceEntryHandler(IAttendanceDbContext dbContext, IStorageManager storageManager)
        {
            _dbContext = dbContext;
            _storageManager = storageManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<UploadFileEventAttendanceEntryRequest>(nameof(UploadFileEventAttendanceEntryRequest.IdEventCheck),
                                                                                      nameof(UploadFileEventAttendanceEntryRequest.IdUserEvent));

            // var file = Request.Form.Files.FirstOrDefault();
            // if (file is null || file.Length == 0)
            //     throw new BadRequestException("Attendance file not provided");

            // var fileInfo = new FileInfo(file.FileName);
            // if (!_allowedFiles.Any(x => x == fileInfo.Extension))
            //     throw new BadRequestException($"File not allowed. Allowed file: {string.Join(", ", _allowedFiles)}");

            // var eventCheck = await _dbContext.Entity<MsEventIntendedForAtdCheckStudent>()
            //                                  .Include(x => x.EventIntendedForAttendanceStudent).ThenInclude(x => x.EventIntendedFor)
            //                                  .SingleOrDefaultAsync(x => x.Id == param.IdEventCheck);

            // if (eventCheck is null)
            //     throw new BadRequestException("Event check is not found");

            // var userEvent = await _dbContext.Entity<MsUserEvent>()
            //                                 .Include(x => x.EventDetail)
            //                                 .SingleOrDefaultAsync(x => x.Id == param.IdUserEvent
            //                                                            && x.EventDetail.IdEvent == eventCheck.EventIntendedForAttendanceStudent.IdEventIntendedFor);
            // if (userEvent is null)
            //     throw new BadRequestException("User event is not found");

            // var blobName = BlobUtil.CreateBlobNameEventAttendance(eventCheck.StartDate, userEvent.IdUser, eventCheck.Id);
            // var blobMetadata = BlobUtil.ToEventAttendanceEntryMetadata(AuthInfo.UserId, file.FileName, file.ContentType);

            // var blobContainer = await _storageManager.GetOrCreateBlobContainer("event-attendance-entry", PublicAccessType.None, CancellationToken);
            // var blob = blobContainer.GetBlobClient(blobName);

            // using var fs = file.OpenReadStream();
            // await blob.UploadAsync(fs, metadata: blobMetadata, cancellationToken: CancellationToken);

            // return Request.CreateApiResult2(blobName as object);
            return Request.CreateApiResult2();
        }
    }
}
