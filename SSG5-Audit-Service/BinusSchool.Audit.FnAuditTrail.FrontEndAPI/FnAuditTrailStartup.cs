using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.AuditDb;
using BinusSchool.Persistence.AuditDb.Abstractions;
using BinusSchool.Audit.FnAuditTrail;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using BinusSchool.Audit.BLL;

[assembly: FunctionsStartup(typeof(FnAuditTrailStartup))]
namespace BinusSchool.Audit.FnAuditTrail
{
    public class FnAuditTrailStartup : FunctionsStartup
    {
        public FnAuditTrailStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
            builder.Services
                .AddFunctionsService<FnAuditTrailStartup>(configuration, false);
                // .AddPersistence<IAuditNoDbContext, AuditDbContext>(configuration, true);
            
            builder.Services.AddFunctionsHealthChecks(configuration, false);
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var hostContext = builder.GetContext();
            
            // Workaround for unstable EnvironmentName in Azure 
            // see https://github.com/Azure/azure-functions-host/issues/6239
            var envName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                hostContext.EnvironmentName;

            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(Audit), envName, hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
