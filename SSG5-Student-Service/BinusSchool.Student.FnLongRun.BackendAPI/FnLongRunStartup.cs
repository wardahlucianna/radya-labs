using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.StudentDb;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Student.FnLongRun;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FnLongRunStartup))]

namespace BinusSchool.Student.FnLongRun
{
    public class FnLongRunStartup : FunctionsStartup
    {
        public FnLongRunStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        private readonly Type[] _consumeApiDomains =
        {
            typeof(Data.Api.Util.IDomainUtil),
            typeof(Data.Api.Student.IDomainStudent)
        };

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            builder.Services
                .AddFunctionsService<FnLongRunStartup, IStudentDbContext>(configuration, false, _consumeApiDomains)
                .AddPersistence<IStudentDbContext, StudentDbContext>(configuration, 600, 1024);

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
            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(Student), envName,
                hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
