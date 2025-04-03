using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Employee.FnStaff;
using BinusSchool.Persistence.EmployeeDb;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Data.Configurations;
using BinusSchool.Data.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BinusSchool.Employee.BLL;

[assembly: FunctionsStartup(typeof(FnStaffStartup))]

namespace BinusSchool.Employee.FnStaff
{
    public class FnStaffStartup : FunctionsStartup
    {
        public FnStaffStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        private readonly Type[] _consumeApiDomains =
        {
            typeof(Data.Api.School.IDomainSchool),
            typeof(Data.Api.Teaching.IDomainTeaching)
        };

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
            builder.Services
                .AddFunctionsService<FnStaffStartup, IEmployeeDbContext>(configuration, false, _consumeApiDomains)
                .AddPersistence<IEmployeeDbContext, EmployeeDbContext>(configuration, 300)
                .AddBinusianApiServiceFactory(configuration.GetSection("BinusianService")
                    .Get<ApiConfigurationWithName>());

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

            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(Employee), envName,
                hostContext.ApplicationRootPath, "BinusianService:*");

            base.ConfigureAppConfiguration(builder);
        }
    }
}

