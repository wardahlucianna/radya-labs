using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnLongRun.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Scheduling.FnLongRun.TimeTriggers
{
    public class EventSchoolHalfDayHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventSchoolHalfDayHandler> _logger;
        private readonly ISchedulingDbContext _dbContext;


        public EventSchoolHalfDayHandler(
            IServiceProvider serviceProvider,
            ISchedulingDbContext dbContext,
            ILogger<EventSchoolHalfDayHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _dbContext = dbContext;
        }

        [FunctionName(nameof(EventSchoolHalfDay))]
        public async Task EventSchoolHalfDay([TimerTrigger(ScheduleLongRunTimerConstant.EventSchoolHalfConstantTimeHalfDay
#if DEBUG
                //, RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
            CancellationToken cancellationToken)
        {
            var schools = await _dbContext.Entity<MsSchool>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var item in schools)
            {
                _logger.LogInformation("Attendance summary for school {Name} started", item.Name);

                var sw = Stopwatch.StartNew();

                using (var scope = _serviceProvider.CreateScope())
                {
                    var attendanceService = scope.ServiceProvider.GetRequiredService<IEventSchoolService>();
                    await attendanceService.GetEventSchool(item.Id, true, cancellationToken);
                }

                sw.Stop();

                _logger.LogInformation("Event school for school {Name} ended for {TotalSeconds}s", item.Name,
                    Math.Round(sw.Elapsed.TotalSeconds));

                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}
