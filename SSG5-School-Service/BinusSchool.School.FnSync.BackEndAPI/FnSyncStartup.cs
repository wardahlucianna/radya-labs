using System;
using BinusSchool.School.FnSync;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.SchoolDb;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using BinusSchool.School.BLL;

[assembly: FunctionsStartup(typeof(FnSyncStartup))]
namespace BinusSchool.School.FnSync
{
    public class FnSyncStartup : FunctionsStartup
    {
        public FnSyncStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
            builder.Services
                .AddFunctionsService<FnSyncStartup>(configuration, false)
                .AddPersistence<ISchoolDbContext, SchoolDbContext>(configuration, int.MaxValue);

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

            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(School), envName, hostContext.ApplicationRootPath, "SyncRefTable:*");

            base.ConfigureAppConfiguration(builder);
        }
    }
}
