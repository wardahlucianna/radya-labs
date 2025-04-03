using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class GetAscNameUngenerateScheduleHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetAscNameUngenerateScheduleHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = Request.ValidateParams<GetAscNameUngenerateScheduleRequest>();

            var items = await _dbContext.Entity<TrAscTimetable>()
                            .Where(e => e.IdAcademicYear == body.IdAcademicYear && e.GeneratedSchedules.Any())
                            .Select(e=>new ItemValueVm
                            {
                                Id = e.Id,
                                Description = e.Name,
                            })
                            .OrderBy(e=>e.Description)
                            .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(items as object);
        }
    }
}
