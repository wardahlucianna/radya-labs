using System;
using System.Collections.Generic;
using System.Linq;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Configurations;

namespace BinusSchool.Data.Api.Extensions
{
    public static class ApiServiceExtension
    {
        /// <summary>
        /// Set configuration for api service, including hostname & headers
        /// </summary>
        /// <param name="apiService">Refit interface service</param>
        /// <param name="apiConfig">Binus School api configuration</param>
        /// <typeparam name="T">Interface of service</typeparam>
        public static IApiService<T> SetConfigurationFrom<T>(this IApiService<T> apiService, BinusSchoolApiConfiguration2 apiConfig)
            where T : class
        {
            var types = typeof(T).GetInterfaces();
            var domainName = types[1].Name.Remove(0, 7);
            var moduleName = types[0].Name.Remove(0, 1);

            if (apiConfig.Function.TryGetValue(domainName, out var domainConfig) && domainConfig.TryGetValue(moduleName, out var moduleConfig))
            {
                // use api manegement host if provided instead of functions host
                var host = apiConfig.Apim?.Host ?? moduleConfig.Host + moduleConfig.Prefix;
                // add header from apim & functions
                var headers = apiConfig.Apim.Header?.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)) ?? Enumerable.Empty<KeyValuePair<string, string>>();
                headers = headers.Concat(moduleConfig.Header?.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)));
                // use timeout from apim if not provided in functions
                var timeout = moduleConfig.Timeout != 0 ? moduleConfig.Timeout : apiConfig.Apim.Timeout;
                
                apiService.With(host, headers.ToArray());
            }

            return apiService;
        }
    }
}