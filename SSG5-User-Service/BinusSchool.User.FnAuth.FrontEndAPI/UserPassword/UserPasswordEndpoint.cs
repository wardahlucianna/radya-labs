using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnAuth.UserPassword
{
    public class UserPasswordEndpoint
    {
        private const string _route = "auth/up";
        private const string _tag = "User Password";

        private readonly UserPasswordHandler _handler;

        public UserPasswordEndpoint(UserPasswordHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(UserPasswordEndpoint.PostUserPassword))]
        [OpenApiOperation(tags: _tag, Summary = "Login with Username & Password", Description = @"
            Steps to fill body:
            - userName: user name
            - password: user password")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UserPasswordRequest), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UserPasswordResult))]
        public Task<IActionResult> PostUserPassword(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, false);
        }
    }
}
