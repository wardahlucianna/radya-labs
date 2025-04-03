using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetTeacherSemestersHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetTeacherSemestersHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTeacherSemestersRequest>(nameof(GetTeacherSemestersRequest.IdUser));
            var predicate = PredicateBuilder.Create<MsLessonTeacher>(x => x.IdUser == param.IdUser);

            if (!string.IsNullOrWhiteSpace(param.IdAcadYear))
                predicate = predicate.And(x => x.Lesson.IdAcademicYear == param.IdAcadYear);

            if (!string.IsNullOrWhiteSpace(param.IdGrade))
                predicate = predicate.And(x => x.Lesson.IdGrade == param.IdGrade);

            var result = await _dbContext.Entity<MsLessonTeacher>()
                                  .Include(x => x.Lesson)
                                  .Where(predicate)
                                  .Select(x => x.Lesson.Semester)
                                  .Distinct()
                                  .OrderBy(x => x).ToListAsync();
            return Request.CreateApiResult2(result as object);
        }
    }
}
