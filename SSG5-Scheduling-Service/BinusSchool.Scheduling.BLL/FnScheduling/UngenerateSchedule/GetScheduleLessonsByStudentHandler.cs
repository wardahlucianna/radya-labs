using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule
{
    public class GetScheduleLessonsByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetScheduleLessonsByStudentHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetScheduleLessonsByStudentRequest>(nameof(GetScheduleLessonsByStudentRequest.IdAscTimetable));

            var result = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                          .Include(x => x.GeneratedScheduleStudent).ThenInclude(x => x.GeneratedScheduleGrade).ThenInclude(x => x.GeneratedSchedule)
                                          .Where(x => x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == param.IdAscTimetable
                                                      && x.GeneratedScheduleStudent.IdStudent == param.IdStudent
                                                      && (param.StartDate.Date >= x.StartPeriod.Date || param.EndDate.Date >= x.StartPeriod.Date))
                                          .GroupBy(x => new { x.ClassID, x.HomeroomName })
                                          .Select(g => new ItemValueVm
                                          {
                                              Id = g.Key.ClassID,
                                              Description = g.Key.HomeroomName
                                          })
                                          .ToListAsync();

            return Request.CreateApiResult2(result as object);
        }
    }
}
