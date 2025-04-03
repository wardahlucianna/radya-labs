using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Util.BLL;
using BinusSchool.Util.FnConverter;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

[assembly: FunctionsStartup(typeof(FnConverterStartup))]

namespace BinusSchool.Util.FnConverter
{
    public class FnConverterStartup : FunctionsStartup
    {
        public FnConverterStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        private readonly Type[] _consumeApiDomains = 
        {
            typeof(Data.Api.School.IDomainSchool),
            typeof(Data.Api.Scheduling.IDomainScheduling),
            typeof(Data.Api.Student.IDomainStudent),
            typeof(Data.Api.Scoring.IDomainScoring),
            typeof(Data.Api.Attendance.IDomainAttendance),
        };

        public override void Configure(IFunctionsHostBuilder builder)
        {
            //test for trigger all CI
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
            builder.Services
                .AddFunctionsService<FnConverterStartup>(configuration, false, _consumeApiDomains)
                .AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));

            builder.Services.AddFunctionsHealthChecks(configuration, false);
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var hostContext = builder.GetContext();

            // Workaround for unstable EnvironmentName in Azure 
            // see https://github.com/Azure/azure-functions-host/issues/6239
            var envName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                          Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                          hostContext.EnvironmentName;

            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(Util), envName,
                hostContext.ApplicationRootPath, "ConnectionStrings:SyncRefTable:*");

            base.ConfigureAppConfiguration(builder);
        }
    }
}
