using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.School.BLL;
using BinusSchool.School.FnLongRun;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FnLongRunStartup))]

namespace BinusSchool.School.FnLongRun
{
    public class FnLongRunStartup : FunctionsStartup
    {
        private readonly Type[] _consumeApiDomains =
        {
            typeof(Data.Api.Util.IDomainUtil),
            typeof(Data.Api.School.IDomainSchool)
        };

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            builder.Services.AddBusinessLogicLayerAPI(configuration);

            builder.Services.AddLogging(); // Pastikan logging diaktifkan

            builder.Services
                .AddFunctionsService<FnLongRunStartup, ISchoolDbContext>(configuration, false, _consumeApiDomains)
                //timeout set to 10 minutes per command
                .AddPersistence<ISchoolDbContext, SchoolDbContext>(configuration, 600, 1024);

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

            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(School), envName,
                hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
