using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace BinusSchool.User.FnUser
{
    public class ApplicationAuthenticationProvider : IAuthenticationProvider
    {
        private static readonly string[] _defaultScopes = new[] { "https://graph.microsoft.com/.default" };
        
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApplicationAuthenticationProvider> _logger;

        public ApplicationAuthenticationProvider(IConfiguration configuration, ILogger<ApplicationAuthenticationProvider> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            _logger.LogInformation("[MsGraph] Invoke {0} {1}", request.Method, request.RequestUri);
            
            var aadInstance = _configuration.GetSection("AzureActiveDirectory:Instance").Get<string>();
            var aadTenantId = _configuration.GetSection("AzureActiveDirectory:TenantId").Get<string>();
            var aadClientId = _configuration.GetSection("AzureActiveDirectory:ClientId").Get<string>();
            var aadClientSecret = _configuration.GetSection("AzureActiveDirectory:ClientSecret").Get<string>();

            if (string.IsNullOrEmpty(aadInstance)) throw new ArgumentNullException(nameof(aadInstance));
            if (string.IsNullOrEmpty(aadTenantId)) throw new ArgumentNullException(nameof(aadTenantId));
            if (string.IsNullOrEmpty(aadClientId)) throw new ArgumentNullException(nameof(aadClientId));
            if (string.IsNullOrEmpty(aadClientSecret)) throw new ArgumentNullException(nameof(aadClientSecret));
            
            var app = ConfidentialClientApplicationBuilder.Create(aadClientId)
                .WithClientSecret(aadClientSecret)
                .WithClientId(aadClientId)
                .WithTenantId(aadTenantId)
                .Build();

            var result = await app.AcquireTokenForClient(_defaultScopes).ExecuteAsync();
            request.Headers.Add("Authorization", result.CreateAuthorizationHeader());
        }
    }
}
