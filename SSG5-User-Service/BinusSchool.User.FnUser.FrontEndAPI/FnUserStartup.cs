using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.UserDb;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.User.BLL;
using BinusSchool.User.FnUser;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using SendGrid.Extensions.DependencyInjection;


[assembly: FunctionsStartup(typeof(FnUserStartup))]
namespace BinusSchool.User.FnUser
{
    public class FnUserStartup : FunctionsStartup
    {
        public FnUserStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        private readonly Type[] _consumeApiDomains =
        {
            typeof(Data.Api.Teaching.IDomainTeaching),
            typeof(Data.Api.School.IDomainSchool),
            typeof(Data.Api.User.IDomainUser),
            typeof(Data.Api.Scheduling.IDomainScheduling),
        };
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
            builder.Services
                .AddFunctionsService<FnUserStartup, IUserDbContext>(configuration, false, _consumeApiDomains)
                .AddPersistence<IUserDbContext, UserDbContext>(configuration)
                // register Azure AD auth provider for type Application
                .AddScoped<IAuthenticationProvider, ApplicationAuthenticationProvider>()
                // register Ms Graph service
                .AddScoped(provider => new GraphServiceClient(provider.GetService<IAuthenticationProvider>()));
            
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
            
            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(User), envName, hostContext.ApplicationRootPath, "AzureActiveDirectory:*");

            base.ConfigureAppConfiguration(builder);
        }
    }
}
