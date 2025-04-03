using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BinusSchool.Auth.Authentications.Jwt
{
    #pragma warning disable 0618
    public class FunctionAuthorizeAttribute : FunctionInvocationFilterAttribute
    #pragma warning restore
    {
        // Access the authentication info.
        protected AuthenticationInfo Auth { get; private set; }

        #pragma warning disable 0618
        public override Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        #pragma warning restore
        {
            //var workItem = executingContext.Arguments.First().Value as HttpRequestMessage;
            HttpRequest message = executingContext.Arguments.First().Value as HttpRequest;

            Auth = new AuthenticationInfo(message);
            if (!Auth.IsValid) 
            {
                message.Headers.Add("AuthorizationStatus", Convert.ToInt32(HttpStatusCode.Unauthorized).ToString());
                message.Headers.Add("AuthorizationMessage", Auth.Message);
            }
            else
            {
                message.Headers.Add("AuthorizationStatus", Convert.ToInt32(HttpStatusCode.Accepted).ToString());
            }

            return base.OnExecutingAsync(executingContext, cancellationToken);
        }
    }
}
