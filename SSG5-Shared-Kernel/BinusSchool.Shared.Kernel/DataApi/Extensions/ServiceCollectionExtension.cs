using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.School;
using BinusSchool.Data.Configurations;
using BinusSchool.Data.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Refit;

namespace BinusSchool.Data.Api.Extensions
{
    public static class ServiceCollectionExtension
    {
        private static Type[] _apiTypes;
        private static IReadOnlyList<Type> _domainTypes;

        private static void FetchApiDomainTypes(IEnumerable<Type> apiDomains)
        {
            if (_apiTypes is {} && apiDomains is null)
                return;

            _apiTypes ??= Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.IsInterface)
                .ToArray();
            _domainTypes ??= _apiTypes
                .Where(x => !x.IsGenericType && x.GetInterfaces().Length == 0)
                .ToList();

            if (apiDomains is {})
                _domainTypes = _domainTypes.Intersect(apiDomains).ToList();
        }

        [Obsolete("Use AddBinusSchoolApiServiceFactory() instead")]
        public static IServiceCollection AddBinusSchoolApiService<T>(this IServiceCollection services, BinusSchoolApiConfiguration2 apiConfig)
            where T : DelegatingHandler
        {
            if (apiConfig?.Function is null)
                return services;

            FetchApiDomainTypes(null);
            foreach (var domain in _domainTypes)
            {
                // find configuration based on domain interface name, with removed 'I'
                var domainName = domain?.Name.Length >= 7 ? domain.Name.Remove(0, 7) : domain.Name;
                if (apiConfig.Function.TryGetValue(domainName, out var domainConfig))
                {
                    // modules represent Azure functions
                    var modules = _apiTypes.Where(x => domain.IsAssignableFrom(x) && x != domain && x.GetInterfaces().Length == 1);

                    foreach (var module in modules)
                    {
                        var moduleName = module.Name.Remove(0, 1);
                        if (domainConfig.TryGetValue(moduleName, out var moduleConfig))
                        {
                            apiConfig.Apim ??= new BinusSchoolApimConfiguration { Timeout = 90 };
                            // use api manegement host if provided instead of functions host
                            var host = (apiConfig.Apim.Host ?? moduleConfig.Host) + moduleConfig.Prefix;
                            // submodules represent http handler of Azure functions
                            var submodules = _apiTypes.Where(m => module.IsAssignableFrom(m) && m != module);

                            foreach (var submodule in submodules)
                            {
                                var iApiService = typeof(IApiService<>).MakeGenericType(submodule);
                                var apiService = typeof(ApiService<>).MakeGenericType(submodule);

                                // // add header from apim & functions
                                // var headers = apiConfig.Apim.Header?.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)) ?? Enumerable.Empty<KeyValuePair<string, string>>();
                                // headers = headers.Concat(moduleConfig.Header?.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)));
                                // use timeout from apim if not provided in functions
                                var timeout = moduleConfig.Timeout != 0 ? moduleConfig.Timeout : apiConfig.Apim.Timeout;

                                services.Add(new ServiceDescriptor(
                                    iApiService,
                                    provider => Activator.CreateInstance(apiService, host, SerializeNamingProperty.CamelCase, provider, typeof(T), timeout),
                                    ServiceLifetime.Transient));
                            }
                        }
                        // throw if domain config unavailable & any submodule
                        else if (_apiTypes.Any(m => module.IsAssignableFrom(m) && m != module))
                        {
                            // throw new KeyNotFoundException($"Configuration for module {moduleName} module (domain {domainName}) not found");
                        }
                    }
                }
                // throw if domain config unavailable
                else
                {
                    //throw new KeyNotFoundException($"Configuration for domain {domainName} not found");
                }
            }

            // register http handler
            services.AddTransient(typeof(T));

            return services;
        }

        public static IServiceCollection AddBinusSchoolApiServiceFactory<T>(this IServiceCollection services, BinusSchoolApiConfiguration2 apiConfig, List<Type> consumeApiDomains)
            where T : DelegatingHandler
        {
            if (apiConfig?.Function is null)
                return services;

            // ensure consumeApiDomains contains IDomainSchool
            // not null consumeApiDomains mean Functions app
            if (consumeApiDomains is {} && !consumeApiDomains.Contains(typeof(IDomainSchool)))
                consumeApiDomains.Add(typeof(IDomainSchool));

            FetchApiDomainTypes(consumeApiDomains);
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
                UrlParameterFormatter = new RefitUrlParameterFormatter()
            };

            foreach (var domain in _domainTypes)
            {
                // find configuration based on domain interface name, with removed 'I'
                var domainName = domain?.Name.Length >= 7 ? domain.Name.Remove(0, 7) : domain.Name;
                if (apiConfig.Function.TryGetValue(domainName, out var domainConfig))
                {
                    // modules represent Azure functions
                    var modules = _apiTypes.Where(x => domain.IsAssignableFrom(x) && x != domain && x.GetInterfaces().Length == 1);
                    foreach (var module in modules)
                    {
                        var moduleName = module.Name.Remove(0, 1);
                        if (domainConfig.TryGetValue(moduleName, out var moduleConfig))
                        {
                            apiConfig.Apim ??= new BinusSchoolApimConfiguration { Timeout = 90 };
                            // use api management host if provided instead of functions host
                            var host = (apiConfig.Apim.Host ?? moduleConfig.Host) + moduleConfig.Prefix;
                            // submodules represent http handler of Azure functions
                            var submodules = _apiTypes.Where(m => module.IsAssignableFrom(m) && m != module);
                            // use timeout from api management if not provided in functions
                            var timeout = moduleConfig.Timeout != 0 ? moduleConfig.Timeout : apiConfig.Apim.Timeout;

                            foreach (var submodule in submodules)
                            {
                                services
                                    .AddRefitClient(submodule, refitSetting)
                                    .ConfigureHttpClient(client =>
                                    {
                                        client.BaseAddress = new Uri(host);
                                        client.Timeout = TimeSpan.FromSeconds(timeout);

                                        // add default headers from api management configurations
                                        if (apiConfig.Apim.Header != null)
                                            foreach (var header in apiConfig.Apim.Header)
                                                client.DefaultRequestHeaders.Add(header.Key, header.Value);

                                        // set default headers from functions configurations
                                        if (moduleConfig.Header != null)
                                            foreach (var header in moduleConfig.Header)
                                                client.DefaultRequestHeaders.Add(header.Key, header.Value);

                                        // set header accept-language
                                        if (client.DefaultRequestHeaders.AcceptLanguage.Count == 0)
                                            client.DefaultRequestHeaders.AcceptLanguage.Add(
                                                new StringWithQualityHeaderValue(
                                                    string.IsNullOrEmpty(Thread.CurrentThread.CurrentCulture.Name)
                                                        ? "en-US"
                                                        : Thread.CurrentThread.CurrentCulture.Name));
                                    })
                                    .AddHttpMessageHandler<T>();
                            }
                        }
                        // throw if domain config unavailable & any submodule
                        else if (_apiTypes.Any(m => module.IsAssignableFrom(m) && m != module))
                        {
                            // throw new KeyNotFoundException($"Configuration for module {moduleName} module (domain {domainName}) not found");
                        }
                    }
                }
                // throw if domain config unavailable
                else
                {
                    // throw new KeyNotFoundException($"Configuration for domain {domainName} not found");
                }
            }

            // register http message handler with transient lifetime
            services.TryAddTransient<T>();

            return services;
        }
    }
}
