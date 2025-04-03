using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministration
{
    public class AttendanceAdministrationGetHomeroomHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public AttendanceAdministrationGetHomeroomHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAdministrationAttendanceHomeroomRequest>(nameof(GetAdministrationAttendanceHomeroomRequest.IdGrade));
            var data = _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Include(x => x.Homeroom)
                    .ThenInclude(x => x.GradePathwayClassroom)
                        .ThenInclude(x => x.GradePathway)
                            .ThenInclude(x => x.Grade)
                .Where(x => x.Homeroom.GradePathwayClassroom.GradePathway.Grade.Id == param.IdGrade)
                .GroupBy(x=> new
                {
                    x.IdHomeroom,
                    x.HomeroomName,
                    x.Homeroom.Semester
                })
                .Select(x => new CodeWithIdVm
                {
                    Id = x.Key.IdHomeroom,
                    Code = string.Format("{0} (Semester - {1})", x.Key.HomeroomName, x.Key.Semester),
                    Description = string.Format("{0} (Semester - {1})",x.Key.HomeroomName,x.Key.Semester)
                }).AsQueryable();
            if (!string.IsNullOrEmpty(param.Search))
            {
                data = data.Where(x => x.Code.Contains(param.Search));
            }
            var res = await data.ToListAsync();
            return Request.CreateApiResult2(res as object);
        }
    }
}
