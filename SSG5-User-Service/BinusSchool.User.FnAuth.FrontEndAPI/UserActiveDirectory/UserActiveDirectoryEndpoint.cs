using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.User.FnAuth.UserActiveDirectory;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnAuth.UserActiveDirectory
{
    public class UserActiveDirectoryEndpoint
    {
        private const string _route = "auth/ad";
        private const string _tag = "User Active Directory";

        private readonly UserActiveDirectoryHandler _handler;

        public UserActiveDirectoryEndpoint(UserActiveDirectoryHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(PostUserActiveDirectory))]
        [OpenApiOperation(tags: _tag, Summary = "Login with Active Directory", Description = @"
            Steps to fill body:
            - idUser: id user from payload POST /user-fn-auth/auth/up
            - token: token from callback login to Active Directory")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UserActiveDirectoryRequest), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UserPasswordResult))]
        public Task<IActionResult> PostUserActiveDirectory(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, false);
        }
    }
}
