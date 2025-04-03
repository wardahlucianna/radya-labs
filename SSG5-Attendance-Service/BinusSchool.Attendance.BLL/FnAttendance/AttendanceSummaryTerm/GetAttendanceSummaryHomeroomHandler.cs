using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryHomeroomHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetAttendanceSummaryHomeroomHandler(IAttendanceDbContext DbContext)
        {
            _dbContext = DbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryHomeroomRequest>();

            var predicate = PredicateBuilder.Create<MsHomeroom>(x => true);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(e => e.IdAcademicYear == param.IdAcademicYear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(e => e.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(e => e.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                predicate = predicate.And(e => e.Semester == param.Semester);

            var GetHomeroom = await _dbContext.Entity<MsHomeroom>()
                                .Include(x => x.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                .Include(e => e.Grade)
                                .Where(predicate)
                                .Select(e=>new GetAttendanceSummaryHomeroomResult
                                {
                                    IdClassroom = e.GradePathwayClassroom.IdClassroom,
                                    IdGrade = e.IdGrade,
                                    Description = e.GradePathwayClassroom.Classroom.Code
                                })
                                .Distinct().ToListAsync(CancellationToken);

            return Request.CreateApiResult2(GetHomeroom as object);
        }
    }
}
