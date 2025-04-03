using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Common.Extensions
{
    public static class ConfigurationExtension
    {
        public static IDictionary<string, T> GetObject<T>(this IConfiguration configuration, params string[] objectKey)
        {
            var configs = new Dictionary<string, T>();
            foreach (var key in objectKey)
            {
                var config = configuration.GetSection(key).Get<T>();
                if (config == null)
                    throw new InvalidOperationException($"There is no {key} option on configurations file");

                configs.Add(key, config);
            }

            return configs;
        }

        public static IDictionary<string, T> GetObject<T>(this IConfiguration configuration, params (string key, Type type)[] obj)
        {
            var configs = new Dictionary<string, T>();
            foreach (var val in obj)
            {
                // throw if val.type not class/subclass of type T
                if (!val.type.IsAssignableFrom(typeof(T)) && !val.type.IsSubclassOf(typeof(T)))
                    throw new InvalidOperationException($"Type {val.type} is not part of {typeof(T)}");

                var config = configuration.GetSection(val.key).Get(val.type);
                if (config == null)
                    throw new InvalidOperationException($"There is no {val} option on configurations file");

                configs.Add(val.key, (T)config);
            }

            return configs;
        }

        public static IConfigurationRoot AddAppSettings(this IConfigurationBuilder builder, string environment, string basePath = null)
        {
            return builder.AddAppSettingsBuilder(environment, basePath).Build();
        }

        public static IConfigurationBuilder AddAppSettingsBuilder(this IConfigurationBuilder builder, string environment, string basePath = null)
        {
            if (basePath != null)
                builder.SetBasePath(basePath);

            return builder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }
    }
}