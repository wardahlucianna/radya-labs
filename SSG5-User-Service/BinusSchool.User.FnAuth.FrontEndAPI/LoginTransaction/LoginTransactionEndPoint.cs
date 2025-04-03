using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.User.FnAuth.LoginTransaction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnAuth.LoginTransaction
{
    public class LoginTransactionEndPoint
    {
        private const string _route = "auth/login-transaction";
        private const string _tag = "Login Transaction Log";

        private readonly AddLoginTransactionHandler _addLoginTransactionHandler;

        public LoginTransactionEndPoint(AddLoginTransactionHandler addLoginTransactionHandler)
        {
            _addLoginTransactionHandler = addLoginTransactionHandler;
        }

        [FunctionName(nameof(LoginTransactionEndPoint.AddLoginTransaction))]
        [OpenApiOperation(tags: _tag, Summary = "Add Login Transaction Log", Description = "Fill 'Action' param with 'login' or 'logout'")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddLoginTransactionRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddLoginTransaction(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _addLoginTransactionHandler.Execute(req, cancellationToken, false);
        }
    }
}
