using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnPeriod.SessionSet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SessionSet;

namespace BinusSchool.Scheduling.FnSchedule.SessionSet
{
    public class SessionSetEndpoint
    {
        private const string _route = "schedule/session-set";
        private const string _tag = "Session Set";

        private readonly SessionSetHandler _handler;
        private readonly GetSessionSetWithLevelGradeHandler _getSessionSetWithLevelGradeHandler;
        public SessionSetEndpoint(SessionSetHandler handler,
            GetSessionSetWithLevelGradeHandler getSessionSetWithLevelGradeHandler)
        {
            _handler = handler;
            _getSessionSetWithLevelGradeHandler = getSessionSetWithLevelGradeHandler;
        }

        [FunctionName(nameof(SessionSetEndpoint.GetSessionSets))]
        [OpenApiOperation(tags: _tag, Summary = "Get Session Set List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetSessionSets(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SessionSetEndpoint.GetSessionWithLevelGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Session Set with level grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetByLevelGradeRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetByLevelGradeRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetByLevelGradeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetSessionWithLevelGrade(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"/with-level-grade")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getSessionSetWithLevelGradeHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(SessionSetEndpoint.DeletesessionFromAsc))]
        [OpenApiOperation(tags: _tag, Summary = "delete session set from asc")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DeleteSessionSetFromAscRequest.SessionSetId), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeletesessionFromAsc(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-session-set-from-asc")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteSessionSetFromAscHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SessionSetEndpoint.AddSessionSet))]
        [OpenApiOperation(tags: _tag, Summary = "Add Session Set")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddSessionSetRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddSessionSet(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SessionSetEndpoint.UpdateSessionSet))]
        [OpenApiOperation(tags: _tag, Summary = "Update Session Set")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateSessionSetRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateSessionSet(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SessionSetEndpoint.DeleteSessionSet))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Session Set")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteSessionSet(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }
    }
}
