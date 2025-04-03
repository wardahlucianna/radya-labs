using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Linq;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetEventScheduleHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetEventScheduleHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEventScheduleRequest>();

            var listEventSchedule = await _dbContext.Entity<TrEventSchedule>()
                                    .IgnoreQueryFilters()
                                    .Where(e => e.Event.IdAcademicYear == param.IdAcademicYear
                                                && !e.IsSyncAttendance
                                                && e.Event.EventIntendedFor
                                                    .Any(x => x.EventIntendedForAttendanceStudents
                                                        .Any(z => z.Type == EventIntendedForAttendanceStudent.All || z.Type == EventIntendedForAttendanceStudent.Mandatory)))
                                    .Select(e => new GetEventScheduleResult
                                    {
                                        IdEvent = e.IdEvent,
                                        IdScheduleLesson = e.IdScheduleLesson,
                                        IdEventSchedule = e.Id,
                                        IsActive = e.IsActive
                                    })
                                    .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(listEventSchedule as object);
        }
    }
}
