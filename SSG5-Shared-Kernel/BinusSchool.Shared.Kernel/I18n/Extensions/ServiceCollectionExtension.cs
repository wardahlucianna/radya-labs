using System.IO;
using System.Linq;
using System.Reflection;
using Askmethat.Aspnet.JsonLocalizer.Extensions;
using Askmethat.Aspnet.JsonLocalizer.JsonOptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.I18n.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddI18n(this IServiceCollection services, JsonLocalizationOptions jsonLocalizationOptions, bool isFunctionsProject = false)
        {
            services.AddJsonLocalization(options =>
            {
                var binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                options.ResourcesPath = isFunctionsProject ? Path.GetFullPath(Path.Combine(binDirectory, "..", jsonLocalizationOptions.ResourcesPath)) : jsonLocalizationOptions.ResourcesPath;
                options.UseBaseName = jsonLocalizationOptions.UseBaseName;
                options.CacheDuration = jsonLocalizationOptions.CacheDuration;
                options.SupportedCultureInfos = jsonLocalizationOptions.SupportedCultureInfos;
                options.FileEncoding = jsonLocalizationOptions.FileEncoding;
                options.IsAbsolutePath = jsonLocalizationOptions.IsAbsolutePath;
                options.LocalizationMode = LocalizationMode.I18n;
            });
            
            if (!isFunctionsProject)
            {
                // not supported on Functions project
                services.Configure<RequestLocalizationOptions>(options => 
                {
                    // set default culture
                    options.DefaultRequestCulture = new RequestCulture(jsonLocalizationOptions.DefaultCulture, jsonLocalizationOptions.DefaultUICulture);

                    var supportedCultures = jsonLocalizationOptions.SupportedCultureInfos.ToList();
                    // formatting numbers, dates, etc
                    options.SupportedCultures = supportedCultures;
                    // ui strings that we have localized
                    options.SupportedUICultures = supportedCultures;
                });
            }
                
            return services;
        }

        public static IServiceCollection AddI18n(this IServiceCollection services, IConfiguration configuration, bool isFunctionsProject = false)
        {
            return services.AddI18n(configuration.GetSection(nameof(JsonLocalizationOptions)).Get<JsonLocalizationOptions>(), isFunctionsProject);
        }
    }
}