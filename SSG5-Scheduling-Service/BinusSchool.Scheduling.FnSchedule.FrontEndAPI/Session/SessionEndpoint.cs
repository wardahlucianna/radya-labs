using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;

namespace BinusSchool.Scheduling.FnSchedule.Session
{
    public class SessionEndpoint
    {
        private const string _route = "schedule/session";
        private const string _tag = "Session";

        [FunctionName(nameof(SessionEndpoint.GetSessionsForAscTimetable))]
        [OpenApiOperation(tags: _tag, Summary = "Get Session List for upload asc time table")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSessionBySessionSetRequest.IdSessionSet), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPathwayResult))]
        public Task<IActionResult> GetSessionsForAscTimetable(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-by-sessionset")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSessionBySessionSetAscTimeTableHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SessionEndpoint.GetSessions))]
        [OpenApiOperation(tags: _tag, Summary = "Get Session List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSessionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]), Description = "Required if idSchool & idSessionSet empty")]
        [OpenApiParameter(nameof(GetSessionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSessionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSessionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSessionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSessionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSessionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSessionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetSessionRequest.IdSchool), In = ParameterLocation.Query, Description = "Required if ids empty")]
        [OpenApiParameter(nameof(GetSessionRequest.IdSessionSet), In = ParameterLocation.Query, Description = "Required if ids empty")]
        [OpenApiParameter(nameof(GetSessionRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSessionRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSessionRequest.IdPathway), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSessionRequest.IdDay), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPathwayResult))]
        public Task<IActionResult> GetSessions(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSessionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SessionEndpoint.GetSessionDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Session Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPathwayResult))]
        public Task<IActionResult> GetSessionDetail(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
          string id,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SessionHandler>();
            return handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(SessionEndpoint.AddSession))]
        [OpenApiOperation(tags: _tag, Summary = "Add Session")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddSessionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddSession(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SessionHandler>();
            return handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(SessionEndpoint.AddSessionFromAsc))]
        [OpenApiOperation(tags: _tag, Summary = "Add Session From Asc")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(List<AddSessionFromAscRequest>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddSessionFromAsc(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route+ "/add-from-asc")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddSessionFromASCHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SessionEndpoint.UpdateSession))]
        [OpenApiOperation(tags: _tag, Summary = "Update Session")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateSessionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateSession(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SessionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SessionEndpoint.DeleteSession))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Session")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteSession(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SessionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SessionEndpoint.CopySession))]
        [OpenApiOperation(tags: _tag, Summary = "Copy Session")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopySessionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CopySession(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/copy")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CopySessionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SessionEndpoint.GetSessionDays))]
        [OpenApiOperation(tags: _tag, Summary = "Session Day")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSessionDayRequest.IdSessionSet), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSessionDayRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSessionDayRequest.SessionId), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSessionDayResult[]))]
        public Task<IActionResult> GetSessionDays(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/day")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSessionDayHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
