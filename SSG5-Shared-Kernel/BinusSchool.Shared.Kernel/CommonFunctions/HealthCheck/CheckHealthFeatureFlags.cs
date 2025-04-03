using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace BinusSchool.Common.Functions.HealthCheck
{
    public class CheckHealthFeatureFlags : IHealthCheck
    {
        private readonly IFeatureManagerSnapshot _featureManager;
        private readonly IConfigurationRefresherProvider _refresherProvider;

        public CheckHealthFeatureFlags(IFeatureManagerSnapshot featureManager, IConfigurationRefresherProvider refresherProvider, ILogger<CheckHealthFeatureFlags> logger)
        {
            _featureManager = featureManager;
            _refresherProvider = refresherProvider;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var canRefresh = await _refresherProvider.Refreshers.First().TryRefreshAsync();
            if (!canRefresh)
                return HealthCheckResult.Unhealthy("Can't refresh configuration from Azure App Configuration");
            
            var featureFlags = new Dictionary<string, object>();
            await foreach (var featureName in _featureManager.GetFeatureNamesAsync().WithCancellation(cancellationToken))
            {
                var flagValue = await _featureManager.IsEnabledAsync(featureName);
                featureFlags.Add(featureName, flagValue);
            }
            
            return HealthCheckResult.Healthy("Successfully get feature flags from Azure App Configuration", featureFlags);
        }
    }
}
