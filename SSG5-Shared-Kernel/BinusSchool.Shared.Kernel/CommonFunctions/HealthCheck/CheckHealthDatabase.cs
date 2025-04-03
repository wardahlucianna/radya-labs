using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Domain.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BinusSchool.Common.Functions.HealthCheck
{
    public class CheckHealthDatabase : IHealthCheck
    {
        private readonly IAppDbContext _dbContext;

        public CheckHealthDatabase(IServiceProvider serviceProvider, ICurrentFunctions currentFunctions)
        {
            if (currentFunctions.DbContext is {})
                _dbContext = serviceProvider.GetService(currentFunctions.DbContext) as IAppDbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (_dbContext is null)
                return HealthCheckResult.Unhealthy("Current Functions didn't have IAppDbContext");
            
            var sw = new Stopwatch();
            sw.Start();

            var canConnect = await _dbContext.DbFacade.CanConnectAsync(cancellationToken);
            sw.Stop();

            return canConnect
                ? HealthCheckResult.Healthy($"Successfully connect to database with {_dbContext.GetType().Name} in {sw.ElapsedMilliseconds}ms")
                : HealthCheckResult.Unhealthy($"Failed connect to database with {_dbContext.GetType().Name}");
        }
    }
}
