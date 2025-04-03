using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Auth.Abstractions;
using BinusSchool.Data.HttpTools;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Data.Api
{
    public class BinusSchoolHttpHandler : HttpLoggingHandler
    {
        private readonly ICurrentUser _currentUser;

        public BinusSchoolHttpHandler(ILogger<BinusSchoolHttpHandler> logger, ICurrentUser currentUser) :
            base(innerHandler: null, logger: logger)
        {
            _currentUser = currentUser;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_currentUser.TryGetAuthorizationHeader(out var authToken))
            {
                // get token from http header when requested from HttpTrigger
                request.Headers.TryAddWithoutValidation("Authorization", authToken);
            }
            else
            {
                // get token of user system from CurrentUser when requested outside of HttpTrigger
                var (_, token) = _currentUser.GetUserSystem();
                request.Headers.TryAddWithoutValidation("Authorization", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
    
    public class BinusSchoolHttpHandler2 : HttpLoggingHandler
    {
        private readonly ICurrentUser _currentUser;

        public BinusSchoolHttpHandler2(ILogger<BinusSchoolHttpHandler2> logger, ICurrentUser currentUser) : base(logger)
        {
            _currentUser = currentUser;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_currentUser.TryGetAuthorizationHeader(out var authToken))
            {
                // get token from http header when requested from HttpTrigger
                request.Headers.TryAddWithoutValidation("Authorization", authToken);
            }
            else
            {
                // get token of user system from CurrentUser when requested outside of HttpTrigger
                var (_, token) = _currentUser.GetUserSystem();
                request.Headers.TryAddWithoutValidation("Authorization", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
