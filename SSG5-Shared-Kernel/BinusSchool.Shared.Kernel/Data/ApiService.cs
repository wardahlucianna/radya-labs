using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.HttpTools;
using BinusSchool.Data.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Refit;

namespace BinusSchool.Data
{
    public class ApiService<T> : IApiService<T> where T : class
    {
        public T Execute => _execute.Value;

        private string _baseEndpoint;
        private IEnumerable<KeyValuePair<string, string>> _headers;

        private readonly TimeSpan _timeout;
        private readonly RefitSettings _refitSettings;
        private readonly Lazy<T> _execute;

        public ApiService(string baseEndpoint, int timeout = 60, params KeyValuePair<string, string>[] headers) :
            this(baseEndpoint, SerializeNamingProperty.CamelCase, null, null, timeout, headers) {}

        public ApiService(string baseEndpoint, SerializeNamingProperty serializeName, int timeout = 60, params KeyValuePair<string, string>[] headers) : 
            this(baseEndpoint, serializeName, null, null, timeout, headers) {}

        public ApiService(string baseEndpoint, SerializeNamingProperty serializeName, IServiceProvider provider, int timeout = 60, params KeyValuePair<string, string>[] headers) : 
            this(baseEndpoint, serializeName, provider, null, timeout, headers) {}

        public ApiService(string baseEndpoint, SerializeNamingProperty serializeName, IServiceProvider provider, Type handler, int timeout = 60, params KeyValuePair<string, string>[] headers)
        {
            _baseEndpoint = baseEndpoint;
            _timeout = TimeSpan.FromSeconds(timeout);
            _headers = headers;
            _execute = new Lazy<T>(() => CreateClient(
                provider is null 
                    ? new HttpLoggingHandler(null, null)
                    : handler is null 
                        ? provider.GetService<HttpLoggingHandler>()
                        : provider.GetService(handler) as DelegatingHandler));
            
            // System.Text.Json
            // _refitSettings = new RefitSettings(
            //     new SystemTextJsonContentSerializer(
            //         new JsonSerializerOptions
            //         {
            //             PropertyNamingPolicy = serializeName switch
            //             {
            //                 SerializeNamingProperty.CamelCase => JsonNamingPolicy.CamelCase,
            //                 SerializeNamingProperty.SnakeCase => JsonSnakeCaseNamingPolicy.Instance,
            //                 _ => null
            //             }
            //         }));

            // Newtonsoft.JSON
            _refitSettings = new RefitSettings(
                new NewtonsoftJsonContentSerializer(
                    new Newtonsoft.Json.JsonSerializerSettings
                    {
                        ContractResolver = serializeName switch
                        {
                            SerializeNamingProperty.CamelCase => new CamelCasePropertyNamesContractResolver(),
                            SerializeNamingProperty.SnakeCase => new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
                            _ => new CamelCasePropertyNamesContractResolver()
                        },
                        Converters = new List<JsonConverter>
                        {
                            new StringEnumConverter()
                        }
                    }
            ))
            {
                CollectionFormat = CollectionFormat.Csv
            };
        }

        public void With(string host = null, params KeyValuePair<string, string>[] headers)
        {
            _baseEndpoint = host ?? _baseEndpoint;
            _headers = _headers.Union(headers);
        }

        private T CreateClient(HttpMessageHandler messageHandler)
        {
            var client = new HttpClient(messageHandler)
            {
                BaseAddress = new Uri(_baseEndpoint),
                Timeout = _timeout
            };

            foreach (var header in _headers)
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);

            if (client.DefaultRequestHeaders.AcceptLanguage.Count == 0)
                client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(string.IsNullOrEmpty(Thread.CurrentThread.CurrentCulture.Name)
                    ? "en-US"
                    : Thread.CurrentThread.CurrentCulture.Name));

            return _refitSettings is null 
                ? RestService.For<T>(client)
                : RestService.For<T>(client, _refitSettings);
        }
    }
}
