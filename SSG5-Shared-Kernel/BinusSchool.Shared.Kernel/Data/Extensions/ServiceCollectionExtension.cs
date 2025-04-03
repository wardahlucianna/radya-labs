using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using BinusSchool.Data.Apis;
using BinusSchool.Data.Serialization;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Apis.Binusian.BinusSchool;
using BinusSchool.Data.Configurations;
using BinusSchool.Data.HttpTools;
using Microsoft.Extensions.Logging;
using Refit;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BinusSchool.Data.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddData<T>(this IServiceCollection services, IDictionary<string, T> apiOption) where T : ApiConfiguration
        {
            #region Register API services
            
            if (apiOption.TryGetValue("GitHubService", out var val) && val is ApiConfigurationWithName gitHub)
            {
                services.AddTransient<IApiService<IGitHubUserApi>>(provider => new ApiService<IGitHubUserApi>(gitHub.Host, SerializeNamingProperty.SnakeCase, provider, gitHub.Timeout));
            }

            #endregion

            return services.AddHttpLoggingHandler();
        }
        
        [Obsolete("Use AddBinusianApiServiceFactory() instead, that use HttpClientFactory")]
        public static IServiceCollection AddBinusianApiService(this IServiceCollection services, ApiConfigurationWithName binusianApiConfig)
        {
            if (binusianApiConfig is null)
                throw new ArgumentNullException(nameof(binusianApiConfig));

            var apis = new[]
            {
                typeof(IAttendanceLog),
                typeof(IAuth)
            };

            foreach (var api in apis)
            {
                var iApiService = typeof(IApiService<>).MakeGenericType(api);
                var apiService = typeof(ApiService<>).MakeGenericType(api);
                
                services.Add(new ServiceDescriptor(
                    iApiService, 
                    provider => Activator.CreateInstance(apiService, binusianApiConfig.Host, SerializeNamingProperty.CamelCase, provider, null, binusianApiConfig.Timeout), 
                    ServiceLifetime.Transient));
            }

            return services.AddHttpLoggingHandler();
        }

        public static IServiceCollection AddBinusianApiServiceFactory(this IServiceCollection services, ApiConfigurationWithName binusianApiConfig)
        {
            if (binusianApiConfig is null)
                throw new ArgumentNullException(nameof(binusianApiConfig));

            var apis = new[]
            {
                typeof(IAttendanceLog),
                typeof(IAuth)
            };
            var refitSetting = new RefitSettings
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer(
                    new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Converters = new List<JsonConverter> { new StringEnumConverter() }
                    }
                ),
                CollectionFormat = CollectionFormat.Csv,
            };

            foreach (var api in apis)
            {
                services
                    .AddRefitClient(api, refitSetting)
                    .ConfigureHttpClient(client =>
                    {
                        client.BaseAddress = new Uri(binusianApiConfig.Host);
                        client.Timeout = TimeSpan.FromSeconds(binusianApiConfig.Timeout);

                        // add Basic header authorization from Secret
                        if (binusianApiConfig.Secret is {} && binusianApiConfig.Secret.TryGetValue("BasicToken", out var basicToken))
                            client.DefaultRequestHeaders.Add("Authorization", $"Basic {basicToken}");
                    })
                    .AddHttpMessageHandler<BinusianHttpHandler>();
            }

            return services.AddTransient<BinusianHttpHandler>();
        }

        private static IServiceCollection AddHttpLoggingHandler(this IServiceCollection services)
        {
            services.TryAddTransient(provider => new HttpLoggingHandler(null, provider.GetService<ILogger<HttpLoggingHandler>>()));
            
            return services;
        }
    }
}
