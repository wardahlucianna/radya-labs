using System;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnLongRun.Interfaces;
using BinusSchool.Attendance.FnLongRun.Services;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnLongRun.HttpTriggers
{
    public class RunAttendanceSummaryBySchoolAndAcademicYearHandler : FunctionsHttpSingleHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public RunAttendanceSummaryBySchoolAndAcademicYearHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<RunManuallyRequest>();

            if (string.IsNullOrWhiteSpace(param.IdSchool))
                throw new Exception("missing id school");

            if (string.IsNullOrWhiteSpace(param.IdAcademicYear))
                throw new Exception("missing id academic year");

            _ = Task.Run(async () =>
            {
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

                    await service.RunAsync(param.IdSchool, param.IdAcademicYear, CancellationToken.None);
                }
            });

            return Task.FromResult(Request.CreateApiResult2(new
            {
                IsRunning = true
            } as object));
        }
    }
}
