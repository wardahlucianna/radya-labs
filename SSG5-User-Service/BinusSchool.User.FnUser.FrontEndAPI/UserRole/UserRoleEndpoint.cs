using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.User.FnUser.UserRole;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnUser.UserRole
{
    public class UserRoleEndpoint
    {
        private const string _route = "userrole";
        private const string _tag = "User Role";

        private readonly AddUserRoleByRoleCodeHandler _addUserRoleByRoleCodeHandler;
        private readonly UpdateAllUserRoleByRoleCodeHandler _updateAllUserRoleByRoleCodeHandler;

        public UserRoleEndpoint(AddUserRoleByRoleCodeHandler addUserRoleByRoleCodeHandler,
            UpdateAllUserRoleByRoleCodeHandler updateAllUserRoleByRoleCodeHandler)
        {
            _addUserRoleByRoleCodeHandler = addUserRoleByRoleCodeHandler;
            _updateAllUserRoleByRoleCodeHandler = updateAllUserRoleByRoleCodeHandler;
        }

        [FunctionName(nameof(UserRoleEndpoint.AddUserRoleByRoleCode))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddUserRoleByRoleCodeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddUserRoleByRoleCode(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-by-rolecode")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _addUserRoleByRoleCodeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UserRoleEndpoint.UpdateAllUserRoleByRoleCode))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateAllUserRoleByRoleCodeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateAllUserRoleByRoleCode(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-all-user-role")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _updateAllUserRoleByRoleCodeHandler.Execute(req, cancellationToken);
        }
    }
}
