using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryWidgeHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceSummaryTerm _apiAttendanceSummaryTerm;

        public GetAttendanceSummaryWidgeHandler(IAttendanceDbContext dbContext, IAttendanceSummaryTerm ApiAttendanceSummaryTerm)
        {
            _dbContext = dbContext;
            _apiAttendanceSummaryTerm = ApiAttendanceSummaryTerm;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryRequest>();

            var GetAttendanceSummaryApi = await _apiAttendanceSummaryTerm.GetAttendanceSummary(
               new GetAttendanceSummaryRequest
               {
                   IdAcademicYear = param.IdAcademicYear,
                   SelectedPosition = param.SelectedPosition,
                   IdUser = param.IdUser,
               });

            if (!GetAttendanceSummaryApi.IsSuccess)
                return Request.CreateApiResult2();

            var LastUpdate = await _dbContext.Entity<TrAttendanceSummaryLog>()
                .Where(x => x.IsDone)
                .OrderByDescending(e=>e.StartDate)
                .Select(e=>e.StartDate)
                .FirstOrDefaultAsync(CancellationToken);

            var GetAttendanceSummary = GetAttendanceSummaryApi.Payload.ToList();
            var Item = new GetAttendanceSummaryWidgeResult
            {
                TotalStudent = GetAttendanceSummary.Select(x => x.TotalStudent).Sum(),
                Submited = GetAttendanceSummary.Select(x => x.Submited).Sum(),
                Unsubmitted = GetAttendanceSummary.Select(x => x.Unsubmitted).Sum(),
                Pending = GetAttendanceSummary.Select(x => x.Pending).Sum(),
                LastUpdate = LastUpdate
            };

            return Request.CreateApiResult2(Item as object);
        }
    }
}
