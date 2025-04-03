using System;
using BinusSchool.Attendance.FnAttendanceSummary;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Data.Configurations;
using BinusSchool.Data.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BinusSchool.Attendance.BLL;

[assembly: FunctionsStartup(typeof(FnAttendanceSummaryStartup))]

namespace BinusSchool.Attendance.FnAttendanceSummary
{
    public class FnAttendanceSummaryStartup : FunctionsStartup
    {
        public FnAttendanceSummaryStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        private readonly Type[] _consumeApiDomains =
        {
            typeof(Data.Api.Attendance.IDomainAttendance),
        };

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
            builder.Services
                .AddFunctionsService<FnAttendanceSummaryStartup, IAttendanceDbContext>(configuration, false,
                    _consumeApiDomains)
                .AddPersistence<IAttendanceDbContext, AttendanceDbContext>(configuration, 300)
                .AddBinusianApiServiceFactory(configuration.GetSection("BinusianService")
                    .Get<ApiConfigurationWithName>());

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
                hostContext.ApplicationRootPath, "BinusianService:*");

            base.ConfigureAppConfiguration(builder);
        }
    }
}
