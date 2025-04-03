using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnLongRun.Interfaces;
using BinusSchool.Attendance.FnLongRun.Services;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnLongRun.TimeTriggers
{
    public class AttendanceSummaryHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<AttendanceSummaryHandler> _logger;

        public AttendanceSummaryHandler(
            IServiceProvider serviceProvider,
            IAttendanceDbContext dbContext,
            ILogger<AttendanceSummaryHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _logger = logger;
        }

        [FunctionName(nameof(AttendanceSummary))]
        public async Task AttendanceSummary([TimerTrigger(AttendanceLongRunTimeConstant.AttendanceSummaryConstantTime
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
                    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<IAttendanceDbContext>();
                    var machineDateTime = scope.ServiceProvider.GetRequiredService<IMachineDateTime>();
                    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var attendanceService = scope.ServiceProvider.GetRequiredService<IAttendanceSummaryV3Service>();
                    var service = new AttendanceSummaryBySchoolV3(dbContext,
                        machineDateTime,
                        configuration,
                        attendanceService,
                        loggerFactory.CreateLogger<AttendanceSummaryBySchoolV3>());

                    await service.RunAsync(item.Id, cancellationToken);
                }

                sw.Stop();

                _logger.LogInformation("Attendance summary for school {Name} ended for {TotalSeconds}s", item.Name,
                    Math.Round(sw.Elapsed.TotalSeconds));

                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}
