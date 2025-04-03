using System;
using System.Collections.Generic;
using System.Linq;
using BinusSchool.Auth;
using BinusSchool.Auth.Abstractions;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Functions.EventHub;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Queue;
using BinusSchool.Common.Functions.Storage;
using BinusSchool.Data.Api;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Configurations;
using BinusSchool.I18n.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Fn = BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Table;
using FunctionHealthCheck;
using BinusSchool.Domain.Abstractions;
using Microsoft.Azure.WebJobs.Hosting;
using BinusSchool.Common.Functions.HealthCheck;
using BinusSchool.Common.Functions.Services;
using Microsoft.FeatureManagement;

namespace BinusSchool.Common.Functions.Extensions
{
    public static class ServiceCollectionExtension
    {
        private static CurrentFunctions _currentFunctions;
        private static Type[] _currentFunctionsTypes;
        private static string _currentDbContextName, _lowerDomainName;

        public static IServiceCollection AddFunctionsService<TStartUp, TDbContext>(this IServiceCollection services,
            IConfiguration configuration,
            bool useLegacyHttpClient = true, Type[] consumeApiDomains = null)
            where TStartUp : IWebJobsStartup2, IWebJobsStartup, IWebJobsConfigurationStartup
            where TDbContext : IAppDbContext
        {
            _currentFunctions = new CurrentFunctions(typeof(TStartUp), typeof(TDbContext));
            _currentDbContextName = typeof(TDbContext).Name;

            return services.AddFunctionsService<TStartUp>(configuration, useLegacyHttpClient, consumeApiDomains);
        }

        public static IServiceCollection AddFunctionsService<TStartUp>(this IServiceCollection services,
            IConfiguration configuration,
            bool useLegacyHttpClient = true, Type[] consumeApiDomains = null)
            where TStartUp : IWebJobsStartup2, IWebJobsStartup, IWebJobsConfigurationStartup
        {
            _currentFunctions ??= new CurrentFunctions(typeof(TStartUp), null);
            _currentFunctionsTypes = typeof(TStartUp).Assembly.GetTypes();

            // save legacy HttpClient value into environment variable
            Environment.SetEnvironmentVariable(EnvironmentConstant.UseHttpClientFactory,
                (!useLegacyHttpClient).ToString());

            if (useLegacyHttpClient)
                services.AddBinusSchoolApiService<BinusSchoolHttpHandler>(configuration.GetSection("BinusSchoolService")
                    .Get<BinusSchoolApiConfiguration2>());
            else
                services.AddBinusSchoolApiServiceFactory<BinusSchoolHttpHandler2>(
                    configuration.GetSection("BinusSchoolService").Get<BinusSchoolApiConfiguration2>(),
                    consumeApiDomains?.ToList() ?? new List<Type>(0));

            services.AddAzureAppConfiguration();
            services.AddFeatureManagement();
            services.AddI18n(configuration, true);
            services.AddSingleton<IMachineDateTime, MachineDateTime>();
            services.AddSingleton<TokenIssuer>();
            services.AddSingleton<ICurrentFunctions>(_currentFunctions);
            services.AddScoped<ICurrentUser, CurrentUser>();

            // register http handler
            services.AddFunctionsHandler(typeof(Fn.IFunctionsHttpHandler), ServiceLifetime.Transient);
            // register notification handler
            services.AddFunctionsHandler(typeof(IFunctionsNotificationHandler), ServiceLifetime.Transient);
            // register queue
            services.AddScoped<IAuditTrail, AuditTrail>();
            // register sync denormalize table
            services.AddScoped<ISyncReferenceTable, SyncReferenceTable>();

            // register storage manager
            services.AddSingleton<IStorageManager, FunctionsStorageManager>();
            // register table manager
            services.AddSingleton<ITableManager, TableManager>();
            // register notification manager
            services.AddSingleton<INotificationManager, NotificationManager>();
            // register redis
            services.AddRedis(configuration);

            // register easy caching
            services.AddEasyCaching(options =>
            {
                // register in memory cache provider
                options.UseInMemory(configure =>
                {
                    configure.EnableLogging = true;
                    configure.DBConfig.SizeLimit = 1000;
                }, CacheProvider.InMemory);
            });

            return services;
        }

        public static IHealthChecksBuilder AddFunctionsHealthChecks(this IServiceCollection services,
            IConfiguration configuration, bool checkSqlServer = true)
        {
            if (_currentFunctions is null)
                throw new InvalidOperationException(
                    "Please call AddFunctionsService method first before calling AddFunctionsHealthChecks method.");

            // add health checks
            var healthCheks = services.AddFunctionHealthChecks();
            _lowerDomainName = _currentFunctions.Domain.ToLower();

            // add sql server check
            if (checkSqlServer)
                healthCheks.AddSqlServer(
                    connectionString: configuration.GetConnectionString($"{_currentFunctions.Domain}:SqlServer"),
                    name: $"sqlserver: BSS_{_currentFunctions.Domain.ToUpper()}_DB",
                    tags: new[] { "sqlserver", _lowerDomainName });

            // add event hub check if sql server checked & current functions is not functions sync
            if (checkSqlServer && _currentFunctions.Functions != "FnSync")
            {
                healthCheks.AddFunctionsHealthChecksEventHub(configuration);

                // add sql server check with EF Core DbContext
                healthCheks.AddCheck<CheckHealthDatabase>($"dbcontext: {_currentDbContextName}",
                    tags: new[] { "dbcontext", _lowerDomainName });
            }

            // add feature flags check
            healthCheks.AddCheck<CheckHealthFeatureFlags>("feature-flags",
                tags: new[] { "featureflags", _lowerDomainName });

            return healthCheks;
        }

        private static IHealthChecksBuilder AddFunctionsHealthChecksEventHub(this IHealthChecksBuilder healthChecks,
            IConfiguration configuration)
        {
            var dbRefs = configuration.GetSection($"ConnectionStrings:{_currentFunctions.Domain}:Refs")
                .Get<IDictionary<string, string[]>>();

            if (dbRefs?.Any(x => x.Value.Length != 0) ?? false)
            {
                foreach (var dbRef in dbRefs)
                {
                    var connString = configuration.GetSection($"ConnectionStrings:SyncRefTable:EventHubs:{dbRef.Key}")
                        .Get<string>();

                    if (string.IsNullOrEmpty(connString))
                        continue;

                    foreach (var dbRefName in dbRef.Value)
                    {
                        var hubName = $"{_currentFunctions.Domain}-{dbRefName}".ToLower();
                        healthChecks.AddAzureEventHub(connString, hubName, $"eventhub: {hubName}",
                            tags: new[] { "eventhub", _lowerDomainName });
                    }
                }
            }

            return healthChecks;
        }

        private static IServiceCollection AddFunctionsHandler(this IServiceCollection services, Type handlerType,
            ServiceLifetime lifetime)
        {
            var handlers = _currentFunctionsTypes
                .Where(x
                    => handlerType.IsAssignableFrom(x)
                       && x != handlerType
                       && !x.IsInterface
                       && !x.IsAbstract);

            foreach (var handler in handlers)
                services.Add(new ServiceDescriptor(handler, handler, lifetime));

            return services;
        }
    }
}
