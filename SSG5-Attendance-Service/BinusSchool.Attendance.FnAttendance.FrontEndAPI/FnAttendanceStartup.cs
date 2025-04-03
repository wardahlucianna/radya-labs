using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Attendance.FnAttendance;
using BinusSchool.Attendance.FnAttendance.Abstractions;
using BinusSchool.Attendance.FnAttendance.Services;
using BinusSchool.Data.Configurations;
using BinusSchool.Data.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BinusSchool.Attendance.BLL;

[assembly: FunctionsStartup(typeof(FnAttendaceStartup))]

namespace BinusSchool.Attendance.FnAttendance
{
    public class FnAttendaceStartup : FunctionsStartup
    {
        public FnAttendaceStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        private readonly Type[] _consumeApiDomains =
        {
            typeof(Data.Api.Attendance.IDomainAttendance),
            typeof(Data.Api.Scheduling.IDomainScheduling),
            typeof(Data.Api.Student.IDomainStudent),
        };

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            builder.Services.AddBusinessLogicLayerAPI(configuration);

            builder.Services
                .AddFunctionsService<FnAttendaceStartup, IAttendanceDbContext>(configuration, false, _consumeApiDomains)
                .AddPersistence<IAttendanceDbContext, AttendanceDbContext>(configuration, 300)
                .AddBinusianApiServiceFactory(configuration.GetSection("BinusianService")
                    .Get<ApiConfigurationWithName>());

            builder.Services.AddFunctionsHealthChecks(configuration);
            builder.Services.AddScoped<IAttendanceSummaryService, AttendanceSummaryService>();
            builder.Services.AddScoped<IAttendanceSummaryRedisService, AttendanceSummaryRedisService>();
            builder.Services.AddScoped<IAttendanceRecapService, AttendanceRecapService>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var hostContext = builder.GetContext();

            // Workaround for unstable EnvironmentName in Azure 
            // see https://github.com/Azure/azure-functions-host/issues/6239
            var envName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                          Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                          hostContext.EnvironmentName;
            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(BinusSchool.Attendance), envName,
                hostContext.ApplicationRootPath, "BinusianService:*");
            base.ConfigureAppConfiguration(builder);
        }
    }
}
