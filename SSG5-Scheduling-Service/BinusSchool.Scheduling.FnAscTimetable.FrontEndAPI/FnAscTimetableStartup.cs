using System;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchedulingDb;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Scheduling.FnAscTimetable;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using BinusSchool.Scheduling.BLL;

[assembly: FunctionsStartup(typeof(FnAscTimetableStartup))]
namespace BinusSchool.Scheduling.FnAscTimetable
{
    public class FnAscTimetableStartup : FunctionsStartup
    {
        public FnAscTimetableStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
            builder.Services
                .AddFunctionsService<FnAscTimetableStartup, ISchedulingDbContext>(configuration)
                .AddPersistence<ISchedulingDbContext, SchedulingDbContext>(configuration, 300);

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

            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(BinusSchool.Scheduling), envName, hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
