using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.User.FnUser.MenuAndPermission;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnUser.MenuAndPermission
{
    public class MenuAndPermissionEndpoint
    {
        private const string _route = "menu-permission";
        private const string _tag = "Menu And Permission";

        [FunctionName(nameof(MenuAndPermissionEndpoint.GetMenuAndPermissions))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(MenuAndPermissionResult))]
        public Task<IActionResult> GetMenuAndPermissions(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMenuAndPermissionHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MenuAndPermissionEndpoint.GetUserMenuAndPermissions))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUserMenuAndPermissionRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserMenuAndPermissionRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserMenuAndPermissionRequest.IdRole), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserMenuAndPermissionRequest.IsMobile), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UserMenuAndPermissionResult))]
        public Task<IActionResult> GetUserMenuAndPermissions(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/user")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUserMenuAndPermissionHandler>();
            return handler.Execute(req, cancellationToken, false);
        }


        [FunctionName(nameof(MenuAndPermissionEndpoint.GetFeatureUserMenuAndPermissions))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUserMenuAndPermissionRequest.IdRole), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(FeatureUserMenuAndPermissionResult))]
        public Task<IActionResult> GetFeatureUserMenuAndPermissions(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/user-feature")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetFeatureUserMenuAndPermissionHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MenuAndPermissionEndpoint.GetMobileMainMenu))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMobileMainMenuResult))]
        public Task<IActionResult> GetMobileMainMenu(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/mobile-main-menu")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMobileMainMenuHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(MenuAndPermissionEndpoint.GetMobileSubMenu))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMobileSubMenuRequest.Id), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMobileSubMenuResult))]
        public Task<IActionResult> GetMobileSubMenu(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/mobile-sub-menu")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMobileSubMenuHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}
