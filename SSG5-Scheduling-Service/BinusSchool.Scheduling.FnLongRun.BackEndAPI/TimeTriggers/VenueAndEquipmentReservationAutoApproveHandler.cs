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
    public class VenueAndEquipmentReservationAutoApproveHandler
    {
        private const string _executionSchedule = "0 15 * * *";
        //private const string _executionSchedule = "*/1 * * * *";

        private readonly IServiceProvider _serviceProvider;
        private readonly ISchedulingDbContext _context;
        private readonly IMachineDateTime _dateTime;
        private readonly ILogger<VenueAndEquipmentReservationAutoApproveHandler> _logger;

        public VenueAndEquipmentReservationAutoApproveHandler(IServiceProvider serviceProvider, ISchedulingDbContext context, IMachineDateTime dateTime, ILogger<VenueAndEquipmentReservationAutoApproveHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _context = context;
            _dateTime = dateTime;
            _logger = logger;
        }

        [FunctionName(nameof(VenueAndEquipmentReservationAutoApprove))]
        public async Task VenueAndEquipmentReservationAutoApprove([TimerTrigger(_executionSchedule)] TimerInfo timerInfo, CancellationToken cancellationToken)
        {
            var schools = await _context.Entity<MsSchool>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var item in schools)
            {
                _logger.LogInformation("Auto approve venue and equipment reservation for school {Name} started", item.Name);

                var sw = Stopwatch.StartNew();

                using (var scope = _serviceProvider.CreateScope())
                {
                    var executeApproval = scope.ServiceProvider.GetRequiredService<IVenueAndEquipmentReservationAutoApproveService>();
                    await executeApproval.VenueAndEquipmentReservationAutoApprove(item.Id, cancellationToken);
                }

                sw.Stop();

                _logger.LogInformation("Auto approve venue and equipment reservation for school {Name} ended", item.Name, Math.Round(sw.Elapsed.TotalSeconds));

                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}
