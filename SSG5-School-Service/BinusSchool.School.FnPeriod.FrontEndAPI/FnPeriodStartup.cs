﻿using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.School.BLL;
using BinusSchool.School.FnPeriod;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FnPeriodStartup))]
namespace BinusSchool.School.FnPeriod
{
    public class FnPeriodStartup : FunctionsStartup
    {
        public FnPeriodStartup()
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
                .AddFunctionsService<FnPeriodStartup, ISchoolDbContext>(configuration, false, _consumeApiDomains)
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

            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(BinusSchool.School), envName, hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
