using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Document.Kernel;

public static class FunctionConfigurationServiceCollection
{
    public static IConfigurationBuilder AddFunctionsConfiguration(this IConfigurationBuilder builder, string domain,
        string environment, string basePath, params string[] withConfig)
    {
        // send domain as environment since domain can have own appsettings
        builder.AddAppSettings(domain, basePath);
        var configuration = builder.Build();

        // get app config connection string from local appsettings.json
        var appConfigConnection = configuration.GetSection($"AppConfig:{environment}").Get<string>();

        builder.AddAzureAppConfiguration(options =>
        {
            options.Connect(appConfigConnection)
                // connection string of each domain (db, reftable, etc.)
                .Select($"ConnectionStrings:{domain}:*")
                // connection string to redis
                .Select("ConnectionStrings:Redis")
                // connection string to send queue message of change from database
                .Select("ConnectionStrings:Audit:AccountStorage")
                // connection string to send queue message to domain Util
                .Select("ConnectionStrings:Util:AccountStorage")
                // connection string to send queue message to domain User
                .Select("ConnectionStrings:User:AccountStorage")
                // connection string to send & handle eventhubs message of change from database source
                .Select("ConnectionStrings:SyncRefTable:EventHubs:*")
                // configuration of Binus School API services
                .Select("BinusSchoolService:*")
                // configuration of client app
                .Select("ClientApp:*");

            // additional config
            foreach (var config in withConfig)
                options.Select(config);

            options.ConfigureRefresh(refreshOptions => refreshOptions
                .Register("ConfigVersion")
                .SetCacheExpiration(TimeSpan.FromMinutes(5)));

            options.UseFeatureFlags(refreshOptions =>
                refreshOptions.CacheExpirationInterval = TimeSpan.FromMinutes(5));

            // use key vault
            if (environment != "Development")
            {
                var credentials = new DefaultAzureCredential();
                options.ConfigureKeyVault(config => config.SetCredential(credentials));
            }
        });

        return builder;
    }
}