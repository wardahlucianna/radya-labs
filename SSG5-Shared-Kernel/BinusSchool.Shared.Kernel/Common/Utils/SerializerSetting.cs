using System;
using System.Collections.Generic;
using BinusSchool.Common.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace BinusSchool.Common.Utils
{
    public static class SerializerSetting
    {
        private static readonly IList<JsonConverter> _jsonConverters = new List<JsonConverter>
        {
            new StringEnumConverter(),
            new MemoryStreamConverter()
        };

        private static readonly IContractResolver _pascalCaseContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy { ProcessDictionaryKeys = false }   
        };
        private static readonly IContractResolver _snakeCaseContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = false }   
        };

        private static readonly JsonSerializerSettings _pascalCaseIgnoreNullSerializer = new JsonSerializerSettings
        {
            ContractResolver = _pascalCaseContractResolver,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = _jsonConverters
        };
        private static readonly JsonSerializerSettings _snakeCaseIgnoreNullSerializer = new JsonSerializerSettings
        {
            ContractResolver = _snakeCaseContractResolver,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = _jsonConverters
        };

        private static readonly JsonSerializerSettings _pascalCaseIncludeNullSerializer = new JsonSerializerSettings
        {
            ContractResolver = _pascalCaseContractResolver,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            Converters = _jsonConverters
        };
        private static readonly JsonSerializerSettings _snakeCaseIncludeNullSerializer = new JsonSerializerSettings
        {
            ContractResolver = _snakeCaseContractResolver,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            Converters = _jsonConverters
        };

        public static JsonSerializerSettings GetJsonSerializer(bool serializeNullProperty, NamingStrategy namingStrategy = NamingStrategy.PascalCase)
        {
            return namingStrategy switch
            {
                NamingStrategy.PascalCase => serializeNullProperty ? _pascalCaseIncludeNullSerializer : _pascalCaseIgnoreNullSerializer,
                NamingStrategy.SnakeCase => serializeNullProperty ? _snakeCaseIncludeNullSerializer : _snakeCaseIgnoreNullSerializer,
                _ => throw new InvalidOperationException()
            };
        }
    }

    public enum NamingStrategy
    {
        PascalCase,
        SnakeCase
    }
}
