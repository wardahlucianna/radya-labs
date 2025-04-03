using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.User.FnUser.LoginAs.ImpersonateUser;
using BinusSchool.Data.Model.User.FnUser.LoginAs.ManageAccessRoleLoginAs;
using BinusSchool.User.FnUser.LoginAs.ImpersonateUser;
using BinusSchool.User.FnUser.LoginAs.ManageAccessRoleLoginAs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnUser.LoginAs
{
    public class LoginAsEndPoint
    {
        private const string _route = "loginAs";
        private const string _tag = "Login As";

        private readonly GetGroupListAccessRoleLoginAsHandler _getGroupListAccessRoleLoginAsHandler;
        private readonly GetListAccessRoleLoginAsHandler _getListAccessRoleLoginAsHandler;
        private readonly AddUpdateAccessRoleLoginAsHandler _addUpdateAccessRoleLoginAsHandler;
        private readonly GetListUserForLoginAsHandler _getListUserForLoginAsHandler;
        private readonly CheckUserNamePasswordImpersonateUserLoginAsHandler _checkUserNamePasswordImpersonateUserLoginAsHandler;
        private readonly ImpersonateUserLoginAsHandler _impersonateUserLoginAsHandler;

        public LoginAsEndPoint(
            GetGroupListAccessRoleLoginAsHandler getGroupListAccessRoleLoginAsHandler,
            GetListAccessRoleLoginAsHandler getListAccessRoleLoginAsHandler,
            AddUpdateAccessRoleLoginAsHandler addUpdateAccessRoleLoginAsHandler,
            GetListUserForLoginAsHandler getListUserForLoginAsHandler,
            CheckUserNamePasswordImpersonateUserLoginAsHandler checkUserNamePasswordImpersonateUserLoginAsHandler,
            ImpersonateUserLoginAsHandler impersonateUserLoginAsHandler
           )
        {
            _getGroupListAccessRoleLoginAsHandler = getGroupListAccessRoleLoginAsHandler;
            _getListAccessRoleLoginAsHandler = getListAccessRoleLoginAsHandler;
            _addUpdateAccessRoleLoginAsHandler = addUpdateAccessRoleLoginAsHandler;
            _getListUserForLoginAsHandler = getListUserForLoginAsHandler;
            _checkUserNamePasswordImpersonateUserLoginAsHandler = checkUserNamePasswordImpersonateUserLoginAsHandler;
            _impersonateUserLoginAsHandler = impersonateUserLoginAsHandler;
        }

        [FunctionName(nameof(LoginAsEndPoint.GetGroupListAccessRoleLoginAs))]
        [OpenApiOperation(tags: _tag, Summary = "Get Group List Access Role LoginAs")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGroupListAccessRoleLoginAsRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGroupListAccessRoleLoginAsRequest.IdRole), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetGroupListAccessRoleLoginAsResult>))]
        public Task<IActionResult> GetGroupListAccessRoleLoginAs(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetGroupListAccessRoleLoginAs")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getGroupListAccessRoleLoginAsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LoginAsEndPoint.GetListAccessRoleLoginAs))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Access Role LoginAs")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListAccessRoleLoginAsRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListAccessRoleLoginAsResult>))]
        public Task<IActionResult> GetListAccessRoleLoginAs(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetListAccessRoleLoginAs")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListAccessRoleLoginAsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LoginAsEndPoint.AddUpdateAccessRoleLoginAs))]
        [OpenApiOperation(tags: _tag, Summary = "Add Update Access Role LoginAs")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddUpdateAccessRoleLoginAsRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddUpdateAccessRoleLoginAs(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/AddUpdateAccessRoleLoginAs")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _addUpdateAccessRoleLoginAsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LoginAsEndPoint.GetListUserForLoginAs))]
        [OpenApiOperation(tags: _tag, Summary = "Get List User For LoginAs")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListUserForLoginAsRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListUserForLoginAsRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListUserForLoginAsRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListUserForLoginAsRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListUserForLoginAsRequest.IdRoleGroup), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListUserForLoginAsRequest.RoleGroupCode), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListUserForLoginAsRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListUserForLoginAsRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListUserForLoginAsRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListUserForLoginAsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListUserForLoginAsResult>))]
        public Task<IActionResult> GetListUserForLoginAs(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/GetListUserForLoginAs")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListUserForLoginAsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LoginAsEndPoint.CheckUserNamePasswordImpersonateUserLoginAs))]
        [OpenApiOperation(tags: _tag, Summary = "Login As with Username & Password (Impersonate)", Description = @"
            Steps to fill body:
            - userName: user name
            - password: user password")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CheckUserNamePasswordImpersonateUserLoginAsRequest), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CheckUserNamePasswordImpersonateUserLoginAsResult))]
        public Task<IActionResult> CheckUserNamePasswordImpersonateUserLoginAs(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/CheckUserNamePasswordImpersonateUserLoginAs")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _checkUserNamePasswordImpersonateUserLoginAsHandler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(LoginAsEndPoint.ImpersonateUserLoginAs))]
        [OpenApiOperation(tags: _tag, Summary = "Login As to Impersonate User")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ImpersonateUserLoginAsRequest), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ImpersonateUserLoginAsResult))]
        public Task<IActionResult> ImpersonateUserLoginAs(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/ImpersonateUserLoginAs")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _impersonateUserLoginAsHandler.Execute(req, cancellationToken, false);
        }
    }
}
