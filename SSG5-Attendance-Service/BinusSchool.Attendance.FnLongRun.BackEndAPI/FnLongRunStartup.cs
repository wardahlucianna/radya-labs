using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.AttendanceDb;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Attendance.FnLongRun;
using BinusSchool.Attendance.FnLongRun.Interfaces;
using BinusSchool.Attendance.FnLongRun.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using BinusSchool.Attendance.BLL;

[assembly: FunctionsStartup(typeof(FnLongRunStartup))]

namespace BinusSchool.Attendance.FnLongRun
{
    public class FnLongRunStartup : FunctionsStartup
    {
        public FnLongRunStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        private readonly Type[] _consumeApiDomains =
        {
            typeof(Data.Api.Util.IDomainUtil),
            typeof(Data.Api.Attendance.IDomainAttendance),
            typeof(Data.Api.Scheduling.IDomainScheduling)
        };

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);

            builder.Services
                .AddFunctionsService<FnLongRunStartup, IAttendanceDbContext>(configuration, false, _consumeApiDomains)
                //timeout set to 10 minutes per command
                .AddPersistence<IAttendanceDbContext, AttendanceDbContext>(configuration, 600, 1024);

            builder.Services.AddScoped<IAttendanceSummaryService, AttendanceSummaryService>();
            builder.Services.AddScoped<IAttendanceSummaryV3Service, AttendanceSummaryV3Service>();

            builder.Services.AddFunctionsHealthChecks(configuration);
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var hostContext = builder.GetContext();

            // Workaround for unstable EnvironmentName in Azure 
            // see https://github.com/Azure/azure-functions-host/issues/6239
            var envName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                          Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                          hostContext.EnvironmentName;

            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(Attendance), envName,
                hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
