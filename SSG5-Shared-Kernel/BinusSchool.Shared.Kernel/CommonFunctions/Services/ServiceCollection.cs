using BinusSchool.Common.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.Common.Functions.Services
{
    public static class ServiceCollection
    {
        public static void AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Redis");

            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            services.AddSingleton<IRedisCache, RedisService>();
        }
    }
}
