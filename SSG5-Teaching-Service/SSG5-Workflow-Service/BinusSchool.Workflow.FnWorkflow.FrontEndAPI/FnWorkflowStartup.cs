using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.WorkflowDb;
using BinusSchool.Persistence.WorkflowDb.Abstractions;
using BinusSchool.Workflow.BLL;
using BinusSchool.Workflow.FnWorkflow;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FnWorkflowStartup))]
namespace BinusSchool.Workflow.FnWorkflow
{
    public class FnWorkflowStartup : FunctionsStartup
    {
        public FnWorkflowStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
             builder.Services
                .AddFunctionsService<FnWorkflowStartup, IWorkflowDbContext>(configuration)
                .AddPersistence<IWorkflowDbContext, WorkflowDbContext>(configuration);
            
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

            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(BinusSchool.Workflow), envName, hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
