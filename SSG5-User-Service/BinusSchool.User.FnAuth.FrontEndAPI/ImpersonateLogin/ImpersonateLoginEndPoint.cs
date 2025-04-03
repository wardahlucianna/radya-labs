using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.User.FnAuth.ImpersonateLogin;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;
using BinusSchool.User.FnAuth.UserPassword;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnAuth.ImpersonateLogin
{
    public class ImpersonateLoginEndPoint
    {
        private const string _route = "auth/impersonate-login";
        private const string _tag = "Impersonate Login";

        private readonly ImpersonateLoginHandler _impersonateLoginHandler;
        private readonly MCB01X7UserPasswordHandler _mCB01X7UserPasswordHandler;

        public ImpersonateLoginEndPoint(ImpersonateLoginHandler impersonateLoginHandler, MCB01X7UserPasswordHandler mCB01X7UserPasswordHandler)
        {
            _impersonateLoginHandler = impersonateLoginHandler;
            _mCB01X7UserPasswordHandler = mCB01X7UserPasswordHandler;
        }

        [FunctionName(nameof(ImpersonateLoginEndPoint.ImpersonateLogin))]
        [OpenApiOperation(tags: _tag, Summary = "Login to Impersonated Account")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ImpersonateLoginRequest), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ImpersonateLoginResult))]
        public Task<IActionResult> ImpersonateLogin(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _impersonateLoginHandler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ImpersonateLoginEndPoint.UserPasswordImpersonateLogin))]
        [OpenApiOperation(tags: _tag, Summary = "Login with Username & Password (Impersonate)", Description = @"
            Steps to fill body:
            - userName: user name
            - password: user password")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(MCB01X7UserPasswordRequest), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(MCB01X7UserPasswordResult))]
        public Task<IActionResult> UserPasswordImpersonateLogin(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/auth/up")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _mCB01X7UserPasswordHandler.Execute(req, cancellationToken, false);
        }
    }
}
