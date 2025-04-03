using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.DocumentDb;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Document.FnDocument;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using BinusSchool.Document.BLL;

[assembly: FunctionsStartup(typeof(FnDocumentStartup))]
namespace BinusSchool.Document.FnDocument
{
    public class FnDocumentStartup : FunctionsStartup
    {
        public FnDocumentStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        private readonly Type[] _consumeApiDomains =
        {
            typeof(Data.Api.Document.IDomainDocument),
            typeof(Data.Api.Util.IDomainUtil),
            typeof(Data.Api.Student.IDomainStudent),
            typeof(Data.Api.School.IDomainSchool)
        };

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
            builder.Services
                .AddFunctionsService<FnDocumentStartup, IDocumentDbContext>(configuration, false, _consumeApiDomains)
                .AddPersistence<IDocumentDbContext, DocumentDbContext>(configuration);
            
            builder.Services.AddFunctionsHealthChecks(configuration);
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var hostContext = builder.GetContext();
            
            // Workaround for unstable EnvironmentName in Azure 
            // see https://github.com/Azure/azure-functions-host/issues/6239
            var envName =
                Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                hostContext.EnvironmentName;
            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(Document), envName, hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
