using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.User.FnUser.NotificationHistory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnUser.NotificationHistory
{
    public class NotificationHistoryEndpoint
    {
        private const string _route = "notification-history";
        private const string _tag = "Notification History";

        [FunctionName(nameof(GetNotificationHistories))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetNotificationHistoryRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetNotificationHistoryRequest.IdSchool), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetNotificationHistoryRequest.UserId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetNotificationHistoryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetNotificationHistoryRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetNotificationHistoryRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetNotificationHistoryRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetNotificationHistoryRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetNotificationHistoryRequest.Type), In = ParameterLocation.Query, Type = typeof(NotificationType?), Description = "General|Message")]
        [OpenApiParameter(nameof(GetNotificationHistoryRequest.IsRead), In = ParameterLocation.Query, Type = typeof(bool?))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetNotificationHistoryResult))]
        public Task<IActionResult> GetNotificationHistories(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetNotificationHistoryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetNotificationBadge))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetNotificationBadgeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetNotificationBadgeRequest.UserId), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(int))]
        public Task<IActionResult> GetNotificationBadge(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/badge")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetNotificationBadgeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UpdateNotificationReadStatus))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateNotificationReadRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateNotificationReadStatus(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/read")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateNotificationReadHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UpdateAllNotificationReadStatus))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateAllNotificationReadRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateAllNotificationReadStatus(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/read-all")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateAllNotificationReadHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
