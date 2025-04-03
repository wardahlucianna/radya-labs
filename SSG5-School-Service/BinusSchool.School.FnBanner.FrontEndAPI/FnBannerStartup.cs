using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.School.BLL;
using BinusSchool.School.FnBanner;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FnBannerStartup))]

namespace BinusSchool.School.FnBanner
{
    public class FnBannerStartup : FunctionsStartup
    {
        public FnBannerStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        private readonly Type[] _consumeApiDomains =
        {
            typeof(Data.Api.Document.IDomainDocument),
        };

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            builder.Services.AddBusinessLogicLayerAPI(configuration);

            builder.Services
                .AddFunctionsService<FnBannerStartup, ISchoolDbContext>(configuration, false, _consumeApiDomains)
                .AddPersistence<ISchoolDbContext, SchoolDbContext>(configuration);

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

            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(BinusSchool.School), envName,
                hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
