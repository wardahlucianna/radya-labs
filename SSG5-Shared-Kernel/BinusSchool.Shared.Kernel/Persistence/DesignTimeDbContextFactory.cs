using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Persistence
{
    public abstract class DesignTimeDbContextFactory<TContext> where TContext : DbContext
    {
        protected abstract TContext CreateDbContextInstance(DbContextOptions<TContext> options);

        public TContext CreateDbContext(string[] args)
        {
            var connStringName = "SqlServer";
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process) ?? "Development";

            var domainName = typeof(TContext).Name.Replace(nameof(DbContext), string.Empty);
            var basePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent!.FullName;
            basePath = Path.GetFullPath(Path.Combine(basePath, "..", ".appconfig"));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile($"appconfig.{environment}.json", optional: false)
                .Build();
            var connectionString = configuration.GetConnectionString($"{domainName}:{connStringName}");
            var builder = new DbContextOptionsBuilder<TContext>().UseSqlServer(connectionString);
            // not work if DbContext doesnt have parameterless constructor
            // var dbContext = (TContext)Activator.CreateInstance(
            //     typeof(TContext),
            //     builder.Options);

            return CreateDbContextInstance(builder.Options);
        }
    }
}
