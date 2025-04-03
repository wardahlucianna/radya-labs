using System;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchedulingDb;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Scheduling.FnExtracurricular;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using BinusSchool.Scheduling.BLL;

[assembly: FunctionsStartup(typeof(FnExtracurricularStartup))]
namespace BinusSchool.Scheduling.FnExtracurricular
{
    public class FnExtracurricularStartup : FunctionsStartup
    {
        public FnExtracurricularStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        private readonly Type[] _consumeApiDomains =
        {
            typeof(Data.Api.Scheduling.IDomainScheduling),
            typeof(Data.Api.Finance.IDomainFinance),
            typeof(Data.Api.Util.IDomainUtil),
            typeof(Data.Api.User.IDomainUser)
        };
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
            builder.Services
                .AddFunctionsService<FnExtracurricularStartup, ISchedulingDbContext>(configuration, false, _consumeApiDomains)
                .AddPersistence<ISchedulingDbContext, SchedulingDbContext>(configuration);
            
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

            //var envName = "Production";

            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(BinusSchool.Scheduling), envName, hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
