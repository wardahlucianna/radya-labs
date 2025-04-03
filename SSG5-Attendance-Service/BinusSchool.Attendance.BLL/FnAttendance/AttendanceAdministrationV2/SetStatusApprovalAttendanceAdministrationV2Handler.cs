using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministration.validator;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.ApprovalAttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class SetStatusApprovalAttendanceAdministrationV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public SetStatusApprovalAttendanceAdministrationV2Handler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetStatusApprovalAttendanceAdministrationRequest, SetStatusApprovalAttendanceAdministrationValidator>();
            List<TrAttendanceEntry> trAttendanceEntries = new List<TrAttendanceEntry>();
            var data = await _dbContext.Entity<TrAttendanceAdministration>()
                        .Include(x => x.Attendance)
                        .Include(x => x.StudentGrade)
                            .ThenInclude(x => x.Student)
                        .Include(x => x.StudentGrade)
                            .ThenInclude(x => x.Grade)
                                .ThenInclude(x => x.Level)
                        .Where(x => x.Id == body.Id)
                        .FirstOrDefaultAsync(CancellationToken);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["AttendanceAdministration"], "Id", body.Id));

            data.StatusApproval = body.IsApproved ? 1 : 2;

            _dbContext.Entity<TrAttendanceAdministration>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
                
            return Request.CreateApiResult2();
        }
    }
}
