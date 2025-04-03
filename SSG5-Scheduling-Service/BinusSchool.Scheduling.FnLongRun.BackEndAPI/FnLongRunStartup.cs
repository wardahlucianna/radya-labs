using System;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchedulingDb;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Scheduling.FnLongRun;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using BinusSchool.Scheduling.FnLongRun.Interfaces;
using BinusSchool.Scheduling.FnLongRun.Services;
using BinusSchool.Scheduling.BLL;

[assembly: FunctionsStartup(typeof(FnLongRunStartup))]
namespace BinusSchool.Scheduling.FnLongRun
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
            typeof(Data.Api.Scheduling.IDomainScheduling), 
            typeof(Data.Api.User.IDomainUser), 
            typeof(Data.Api.Teaching.IDomainTeaching), 
        };
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
            builder.Services
                .AddFunctionsService<FnLongRunStartup, ISchedulingDbContext>(configuration, false, _consumeApiDomains)
                .AddPersistence<ISchedulingDbContext, SchedulingDbContext>(configuration, 600, 1024);

            builder.Services.AddScoped<IEventSchoolService, EventSchoolService>();
            builder.Services.AddScoped<IVenueAndEquipmentReservationAutoApproveService, VenueAndEquipmentReservationAutoApproveService>();
            builder.Services.AddScoped<IUpdateVenueReservationOverlapStatusService, UpdateVenueReservationOverlapStatusService>();

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

            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(Scheduling), envName, hostContext.ApplicationRootPath);

            base.ConfigureAppConfiguration(builder);
        }
    }
}
