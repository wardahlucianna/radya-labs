using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.User.FnUser.Role;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnUser.Role
{
    public class RoleEndpoint
    {
        private const string _route = "user/role";
        private const string _routeGroup = _route + "/group";
        private const string _tag = "User Role";

        [FunctionName(nameof(RoleEndpoint.GetRoleGroups))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(RoleResult))]
        public Task<IActionResult> GetRoleGroups(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _routeGroup)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetRoleGroupsHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(RoleEndpoint.GetRolesByCode))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(IdCollection.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetRolesByCode(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _routeGroup + "/by-code")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetRoleGroupByCodeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(RoleEndpoint.GetRolesById))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(IdCollection.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetRolesById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _routeGroup + "/by-id")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetRoleGroupByIdHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(RoleEndpoint.GetRoles))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetRoleRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetRoleRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetRoleRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetRoleRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetRoleRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetRoleRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetRoleRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetRoleRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetRoleRequest.HasPosition), In = ParameterLocation.Query, Type = typeof(bool?))]
        [OpenApiParameter(nameof(GetRoleRequest.IdRoleGroups), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(RoleResult))]
        public Task<IActionResult> GetRoles(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<RoleHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(RoleEndpoint.GetRoleDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(RoleDetailResult))]
        public Task<IActionResult> GetRoleDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<RoleHandler>();
            return handler.Execute(req, cancellationToken, false, "id".WithValue(id));
        }

        [FunctionName(nameof(RoleEndpoint.AddRole))]
        [OpenApiOperation(tags: _tag, Summary = "Add Role")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddRoleRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddRole(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<RoleHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(RoleEndpoint.UpdateRole))]
        [OpenApiOperation(tags: _tag, Summary = "Update Role")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateRoleRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateRole(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<RoleHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(RoleEndpoint.DeleteRole))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Role")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteRole(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<RoleHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(RoleEndpoint.GetRolePosition))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetRolePositionRequest.IdRole), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetRolePositionRequest.IdRoleGroup), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetRolePositionRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetRolePositionHandlerResult))]
        public Task<IActionResult> GetRolePosition(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/position")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetRolePositionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(RoleEndpoint.GetRoleByIdGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Get role by Id Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetRoleByIdGroupRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<CodeWithIdVm>))]
        public Task<IActionResult> GetRoleByIdGroup(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/by-idgroup")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetRoleByIdGroupHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(RoleEndpoint.GetStaffRoles))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStaffRoleRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStaffRoleRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStaffRoleRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStaffRoleRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStaffRoleRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStaffRoleRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStaffRoleRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStaffRoleRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetStaffRoleRequest.HasPosition), In = ParameterLocation.Query, Type = typeof(bool?))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStaffRoleResult))]
        public Task<IActionResult> GetStaffRoles(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-staff")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStaffRoleHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(RoleEndpoint.GetMobileRoleDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetRoleMobileResult))]
        public Task<IActionResult> GetMobileRoleDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/mobile-detail/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<RoleMobileHandler>();
            return handler.Execute(req, cancellationToken, false, "id".WithValue(id));
        }

        [FunctionName(nameof(RoleEndpoint.GetMobileDefaultRoleDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMobileDefaultRoleRequest.IdRoleGroup), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMobileDefaultRoleResult))]

        public Task<IActionResult> GetMobileDefaultRoleDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/mobile-default-role")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMobileDefaultRoleHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(RoleEndpoint.GetRoleMobileType))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetRoleMobileTypeRequest.IdFeature), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetRoleMobileTypeResult>))]
        public Task<IActionResult> GetRoleMobileType(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/role-type")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetRoleMobileTypeHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
