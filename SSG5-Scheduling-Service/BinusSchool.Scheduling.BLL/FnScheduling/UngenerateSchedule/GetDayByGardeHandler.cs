using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.SchedulingDb.Entities;
using System.Linq;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule
{
    public class GetDayByGardeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetDayByGardeHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDayByGardeRequest>();

            var listDays = await _dbContext.Entity<MsScheduleLesson>()
                            .Include(e => e.Day)
                            .Where(x => x.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == param.IdAscTimetable
                                        && x.IdGrade == param.IdGrade
                                        && (x.ScheduleDate.Date >= param.StartDate.Date && x.ScheduleDate.Date <= param.EndDate.Date))
                            .GroupBy(e => new
                            {
                                IdDay = e.IdDay,
                                Day = e.Day.Description,
                            })
                            .Select(e => new ItemValueVm
                            {
                                Id = e.Key.IdDay,
                                Description = e.Key.Day,
                            })
                            .OrderBy(e=>e.Id)
                            .ToListAsync();

            return Request.CreateApiResult2(listDays as object);
        }
    }
}
