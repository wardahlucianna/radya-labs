using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.UserDb;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.User.BLL;
using BinusSchool.User.FnCommunication;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FnCommunicationStartup))]
namespace BinusSchool.User.FnCommunication
{
    public class FnCommunicationStartup : FunctionsStartup
    {
        public FnCommunicationStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        private readonly Type[] _consumeApiDomains =
        {
            typeof(Data.Api.School.IDomainSchool),
            typeof(Data.Api.Scheduling.IDomainScheduling),
            typeof(Data.Api.User.IDomainUser),
        };
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
            builder.Services
                .AddFunctionsService<FnCommunicationStartup, IUserDbContext>(configuration, false, _consumeApiDomains)
                .AddPersistence<IUserDbContext, UserDbContext>(configuration);
            
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
            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(User), envName, hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
