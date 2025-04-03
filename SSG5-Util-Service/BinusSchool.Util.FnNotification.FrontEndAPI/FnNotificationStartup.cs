using System;
using System.IO;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Util.BLL;
using BinusSchool.Util.FnNotification;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Configurations.AppSettings.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FnNotificationStartup))]
namespace BinusSchool.Util.FnNotification
{
    public class FnNotificationStartup : FunctionsStartup
    {
        public FnNotificationStartup()
        {
            //added to update the function for logging purpose
            //update appdbcontext, remove audittrail
        }
        
        private const string _sendGridApi = "SendGridService:Secret:ApiKey";
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddBusinessLogicLayerAPI(configuration);
            builder.Services
                .AddFunctionsService<FnNotificationStartup>(configuration, false)
                .AddSendGrid((s, o) => o.ApiKey = s.GetService<IConfiguration>().GetSection(_sendGridApi).Get<string>());
            
            builder.Services
                .AddFunctionsHealthChecks(configuration, false)
                .AddSendGrid(configuration.GetSection(_sendGridApi).Get<string>(), "sendgrid: notification");

            builder.Services.AddLogging();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var hostContext = builder.GetContext();
            
            // Workaround for unstable EnvironmentName in Azure 
            // see https://github.com/Azure/azure-functions-host/issues/6239
            var envName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                hostContext.EnvironmentName;

            builder.ConfigurationBuilder.AddFunctionsConfiguration(nameof(Util), envName, hostContext.ApplicationRootPath, "SendGridService:Secret:*");

            // initialize Firebase admin
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(Path.Combine(hostContext.ApplicationRootPath, "firebase.json"))
            });

            base.ConfigureAppConfiguration(builder);
        }
    }
}
