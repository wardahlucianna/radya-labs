﻿using System;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.TeachingDb;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Teaching.BLL;
using BinusSchool.Teaching.FnAssignment;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FnAssignmentStartup))]
namespace BinusSchool.Teaching.FnAssignment
{
    public class FnAssignmentStartup : FunctionsStartup
    {
        public FnAssignmentStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }

        private readonly Type[] _consumeApiDomains =
        {
            typeof(Data.Api.User.IDomainUser),
            typeof(Data.Api.Scheduling.IDomainScheduling),
        };

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);

            builder.Services
                .AddFunctionsService<FnAssignmentStartup, ITeachingDbContext>(configuration, false, _consumeApiDomains)
                .AddPersistence<ITeachingDbContext, TeachingDbContext>(configuration);
            
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
            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(BinusSchool.Teaching), envName, hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
