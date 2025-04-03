using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.Kernel;

public static class ServiceCollection
{
    public static IConfigurationRoot AddAppSettings(this IConfigurationBuilder builder, string environment,
        string basePath = null)
    {
        return builder.AddAppSettingsBuilder(environment, basePath).Build();
    }

    public static IConfigurationBuilder AddAppSettingsBuilder(this IConfigurationBuilder builder, string environment,
        string basePath = null)
    {
        if (basePath != null)
            builder.SetBasePath(basePath);

        return builder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
}