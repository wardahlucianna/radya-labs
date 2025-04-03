using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.StudentDb;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Student.BLL;
using BinusSchool.Student.FnGuidanceCounseling;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FnGuidanceCounselingStartup))]
namespace BinusSchool.Student.FnGuidanceCounseling
{
    public class FnGuidanceCounselingStartup : FunctionsStartup
    {
        public FnGuidanceCounselingStartup()
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
                .AddFunctionsService<FnGuidanceCounselingStartup, IStudentDbContext>(configuration, false, _consumeApiDomains)
                .AddPersistence<IStudentDbContext, StudentDbContext>(configuration);
            
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
            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(Student), envName, hostContext.ApplicationRootPath, "ConnectionStrings:Document:AccountStorage");

            base.ConfigureAppConfiguration(builder);
        }
    }
}
