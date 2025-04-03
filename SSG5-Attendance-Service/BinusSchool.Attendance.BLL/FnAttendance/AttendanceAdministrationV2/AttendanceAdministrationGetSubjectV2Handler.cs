

using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class AttendanceAdministrationGetSubjectV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public AttendanceAdministrationGetSubjectV2Handler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAdministrationAttendanceSubjectRequest>(nameof(GetAdministrationAttendanceSubjectRequest.IdHomeroom));

            var queryHomeroomStudentEnrollment = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                .Include(e=>e.Subject)
                                                .Where(e=>e.HomeroomStudent.IdHomeroom==param.IdHomeroom);

            if (!string.IsNullOrEmpty(param.IdUserStudent))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.HomeroomStudent.IdStudent == param.IdUserStudent);

            var listHomeroomStudentEnrollment = await queryHomeroomStudentEnrollment
                                            .GroupBy(e => new
                                            {
                                                e.IdLesson,
                                                e.IdSubject,
                                                e.Subject.Code,
                                                e.Subject.Description

                                            })
                                            .Select(e => e.Key)
                                            .ToListAsync(CancellationToken);

            var listIdLesson = listHomeroomStudentEnrollment.Select(e => e.IdLesson).ToList();

            var dataSubject = _dbContext.Entity<MsScheduleLesson>()
                            .Include(e=>e.Subject)
                            .Where(e => listIdLesson.Contains(e.IdLesson))
                            .GroupBy(e => new CodeWithIdVm
                            {
                                Id = e.IdSubject,
                                Code = e.Subject.Code,
                                Description = e.Subject.Description
                            })
                            .Select(e => e.Key);

        
            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                dataSubject = dataSubject.Where(x => !string.IsNullOrEmpty(param.Search) ? x.Description.Contains(param.Search) : true);
            }
            var res = await dataSubject.ToListAsync(CancellationToken);
            return Request.CreateApiResult2(res as object);
        }
    }
}
