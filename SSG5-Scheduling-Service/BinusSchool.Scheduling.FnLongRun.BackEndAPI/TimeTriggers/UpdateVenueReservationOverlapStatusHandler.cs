using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnLongRun.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Scheduling.FnLongRun.TimeTriggers
{
    public class UpdateVenueReservationOverlapStatusHandler
    {
        private const string _executionSchedule = "0 0,5,11,17,23 * * *";
        //private const string _executionSchedule = "*/1 * * * *";

        private readonly IServiceProvider _serviceProvider;
        private readonly ISchedulingDbContext _context;
        private readonly ILogger<UpdateVenueReservationOverlapStatusHandler> _logger;

        public UpdateVenueReservationOverlapStatusHandler(IServiceProvider serviceProvider, ISchedulingDbContext context, ILogger<UpdateVenueReservationOverlapStatusHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _logger = logger;
        }

        [FunctionName(nameof(UpdateVenueReservationOverlapStatus))]
        public async Task UpdateVenueReservationOverlapStatus([TimerTrigger(_executionSchedule)] TimerInfo timerInfo, CancellationToken cancellationToken)
        {
            var schools = await _context.Entity<MsSchool>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var item in schools)
            {
                _logger.LogInformation("Update venue reservation overlap status for school {Name} started", item.Name);

                var sw = Stopwatch.StartNew();

                using (var scope = _serviceProvider.CreateScope())
                {
                    var executeApproval = scope.ServiceProvider.GetRequiredService<IUpdateVenueReservationOverlapStatusService>();
                    await executeApproval.UpdateVenueReservationOverlapStstus(item.Id, cancellationToken);
                }

                sw.Stop();

                _logger.LogInformation("Update venue reservation overlap status for school {Name} ended", item.Name, Math.Round(sw.Elapsed.TotalSeconds));

                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}
