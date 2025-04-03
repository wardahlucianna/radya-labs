using BinusSchool.Teaching.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.Teaching.BLL;

public static class ServiceCollection
{
    public static void AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence<IAppDbContext, ApplicationDbContext>(configuration, timeout: 300);
    }

    public static IServiceCollection AddPersistence<TIContext, TContext>(this IServiceCollection services,
        IConfiguration configuration, int timeout = 30, int poolSize = 128, bool isNoSql = false)
        where TIContext : IAppDbContext
        where TContext : DbContext, IAppDbContext
    {
        var connString = configuration.GetConnectionString("Audit:SqlServer");

        //you can replace connstring to prod from here
        //connString = "";

        services.AddDbContextPool<TContext>(
            options => options.UseSqlServer(connString, sqlOptions => sqlOptions.CommandTimeout(timeout)), poolSize);
        services.AddScoped(typeof(TIContext), provider => provider.GetService<TContext>()!);

        return services;
    }
}