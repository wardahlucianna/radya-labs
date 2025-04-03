using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnLongRun.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.Scheduling.FnLongRun.TimeTriggers
{
    public class EventSchoolHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventSchoolHandler> _logger;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public EventSchoolHandler(
            IServiceProvider serviceProvider,
            ISchedulingDbContext dbContext,
            IMachineDateTime dateTime,
            ILogger<EventSchoolHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        [FunctionName(nameof(EventSchool))]
        public async Task EventSchool([TimerTrigger(ScheduleLongRunTimerConstant.EventSchoolConstantTime
#if DEBUG
                //, RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
            CancellationToken cancellationToken)
        {
            var envName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var schools = await _dbContext.Entity<MsSchool>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation($"time IMachine {_dateTime.ServerTime}");

            foreach (var item in schools)
            {
                _logger.LogInformation("Evet School for school {Name} started", item.Name);

                var sw = Stopwatch.StartNew();

                using (var scope = _serviceProvider.CreateScope())
                {
                    var schoolEventService = scope.ServiceProvider.GetRequiredService<IEventSchoolService>();
                    await schoolEventService.GetEventSchool(item.Id, false, cancellationToken);
                }

                sw.Stop();

                _logger.LogInformation("Event school for school {Name} ended for {TotalSeconds}s", item.Name,
                    Math.Round(sw.Elapsed.TotalSeconds));

                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}
