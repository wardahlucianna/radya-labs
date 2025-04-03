using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnLongRun.Interfaces;
using BinusSchool.Attendance.FnLongRun.Service;
using BinusSchool.Attendance.FnLongRun.Services;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using HandlebarsDotNet;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Writers;
using NPOI.OpenXmlFormats.Spreadsheet;

namespace BinusSchool.Attendance.FnLongRun.TimeTriggers
{
    public class SchoolEventAttendanceHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly ILogger<SchoolEventAttendanceHandler> _logger;
        private readonly IEventSchool _apiEventSchool;


        public SchoolEventAttendanceHandler(
            IServiceProvider serviceProvider,
            IAttendanceDbContext dbContext,
            IMachineDateTime datetime,
            ILogger<SchoolEventAttendanceHandler> logger,
            IEventSchool apiEventSchool)
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _logger = logger;
            _datetime = datetime;
            _apiEventSchool = apiEventSchool;
        }

        [FunctionName(nameof(SchoolEventAttendance))]
        public async Task SchoolEventAttendance([TimerTrigger(AttendanceLongRunTimeConstant.SchoolEventConstantTime
#if DEBUG
                //, RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("time merit demerit reset", _datetime.ServerTime);

            var schools = await _dbContext.Entity<MsSchool>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var item in schools)
            {
                _logger.LogInformation("School event attendance for school {Name} started", item.Name);
                var sw = Stopwatch.StartNew();
                using (var scope = _serviceProvider.CreateScope())
                {
                    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<IAttendanceDbContext>();
                    var machineDateTime = scope.ServiceProvider.GetRequiredService<IMachineDateTime>();
                    var eventSchool = scope.ServiceProvider.GetRequiredService<IEventSchool>();

                    var service = new SchoolEventAttendanceService(dbContext,
                        machineDateTime,
                        loggerFactory.CreateLogger<SchoolEventAttendanceService>(), eventSchool);

                    await service.RunAsync(item.Id, cancellationToken);
                }

                sw.Stop();

                _logger.LogInformation("School event attendance for school {Name} ended for {TotalSeconds}s", item.Name,
                    Math.Round(sw.Elapsed.TotalSeconds));

                await Task.Delay(5000, cancellationToken);
            }

        }
    }
}
