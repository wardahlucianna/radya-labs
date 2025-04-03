using System;
using System.Linq;
using BinusSchool.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.Persistence.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddPersistence<TIContext, TContext>(this IServiceCollection services, IConfiguration configuration, int timeout = 30, int poolSize = 128, bool isNoSql = false)
            where TIContext : IAppDbContext
            where TContext : DbContext, IAppDbContext
        {
            var connStringName = isNoSql ? "CosmosDb" : "SqlServer";
            var domainName = typeof(TContext).Name.Replace(nameof(DbContext), null);

            if (isNoSql)
            {
                var accountEndpoint = configuration.GetConnectionString($"{domainName}:CosmosDbEndpoint");
                var accountKey = configuration.GetConnectionString($"{domainName}:CosmosDbKey");
                services.AddDbContextPool<TContext>(options => options.UseCosmos(accountEndpoint, accountKey, domainName.ToUpper() + "_DB"), poolSize);
            }
            else
            {
                var connString = configuration.GetConnectionString($"{domainName}:{connStringName}");
                services.AddDbContextPool<TContext>(options => options.UseSqlServer(connString, sqlOptions => sqlOptions.CommandTimeout(timeout)), poolSize);
            }
            services.AddScoped(typeof(TIContext), provider => provider.GetService<TContext>());

            return services;
        }
    }
}
