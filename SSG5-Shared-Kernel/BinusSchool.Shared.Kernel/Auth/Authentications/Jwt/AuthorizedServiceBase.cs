using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BinusSchool.Auth.Authentications.Jwt
{
    #pragma warning disable 0618
    public abstract class AuthorizedServiceBase : IFunctionInvocationFilter
    #pragma warning restore
    {
        private const string _AuthenticationHeaderName = "Authorization";

        // Access the authentication info.
        protected AuthenticationInfo Auth { get; private set; }

        #pragma warning disable 0618
        public Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        #pragma warning restore
        {
            return Task.CompletedTask;
        }

        #pragma warning disable 0618
        public Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        #pragma warning restore
        {
            HttpRequest message = executingContext.Arguments.First().Value as HttpRequest;

            if (message == null || !message.Headers.ContainsKey(_AuthenticationHeaderName))
            {
                //return Task.FromException(new AuthenticationException("No Authorization header was present"));
                return Task.FromResult(new UnauthorizedResult());
            }

            try
            {
                Auth = new AuthenticationInfo(message);
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }

            if (!Auth.IsValid)
            {
                return Task.FromException(new UnauthorizedAccessException(Auth.Message));
            }

            return Task.CompletedTask;
        }
    }
}
