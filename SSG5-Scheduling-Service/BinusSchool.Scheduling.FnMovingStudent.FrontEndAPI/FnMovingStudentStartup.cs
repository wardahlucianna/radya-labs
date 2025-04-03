using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchedulingDb;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Scheduling.BLL;
using BinusSchool.Scheduling.FnMovingSubject;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FnMovingStudentStartup))]

namespace BinusSchool.Scheduling.FnMovingSubject
{
    public class FnMovingStudentStartup : FunctionsStartup
    {
        public FnMovingStudentStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }

        private readonly Type[] _consumeApiDomains =
        {
            typeof(Data.Api.Scheduling.IDomainScheduling),
        };

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
            builder.Services
                .AddFunctionsService<FnMovingStudentStartup, ISchedulingDbContext>(configuration, false,
                    _consumeApiDomains)
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
            
            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(BinusSchool.Scheduling), envName,
                hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
