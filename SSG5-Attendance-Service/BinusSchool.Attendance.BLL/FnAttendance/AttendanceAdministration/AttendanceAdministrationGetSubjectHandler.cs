using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministration
{
    public class AttendanceAdministrationGetSubjectHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public AttendanceAdministrationGetSubjectHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAdministrationAttendanceSubjectRequest>(nameof(GetAdministrationAttendanceSubjectRequest.IdHomeroom));
            var dataSubject =
                (
                    from _subject in _dbContext.Entity<MsSubject>()
                    join _lesson in _dbContext.Entity<TrGeneratedScheduleLesson>() on _subject.Id equals _lesson.IdSubject
                    where
                    _lesson.IdHomeroom == param.IdHomeroom
                    group _subject by new
                    {
                        _subject.Id,
                        _subject.Code,
                        _subject.Description
                    } into g
                    select new CodeWithIdVm
                    {
                        Id = g.Key.Id,
                        Code = g.Key.Code,
                        Description = g.Key.Description
                    }
                ).AsQueryable();
            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                dataSubject = dataSubject.Where(x => !string.IsNullOrEmpty(param.Search) ? x.Description.Contains(param.Search) : true);
            }
            var res = await dataSubject.ToListAsync();
            return Request.CreateApiResult2(res as object);
        }
    }
}
