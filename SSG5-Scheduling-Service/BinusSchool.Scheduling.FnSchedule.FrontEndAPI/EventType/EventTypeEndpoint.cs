using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EventType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.EventType
{
    public class EventTypeEndpoint
    {
        private const string _route = "schedule/event-type";
        private const string _tag = "Event Type";

        private readonly EventTypeHandler _handler;
        private readonly GetEventbyEventTypeHandler _getEventNamebyEventTypeHandler;
        public EventTypeEndpoint(EventTypeHandler handler,
                 GetEventbyEventTypeHandler getEventNamebyEventTypeHandler)
        {
            _handler = handler;
            _getEventNamebyEventTypeHandler = getEventNamebyEventTypeHandler;
        }

        [FunctionName(nameof(EventTypeEndpoint.GetEventTypes))]
        [OpenApiOperation(tags: _tag, Summary = "Get Event Type List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEventTypeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventTypeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventTypeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetEventTypeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetEventTypeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventTypeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventTypeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetEventTypeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEventTypeRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(EventTypeResult))]
        public Task<IActionResult> GetEventTypes(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventTypeEndpoint.GetEventTypeDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Event Type Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(EventTypeResult))]
        public Task<IActionResult> GetEventTypeDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(EventTypeEndpoint.AddEventType))]
        [OpenApiOperation(tags: _tag, Summary = "Add Event Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddEventTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddEventType(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(EventTypeEndpoint.UpdateEventType))]
        [OpenApiOperation(tags: _tag, Summary = "Update Event Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateEventTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateEventType(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EventTypeEndpoint.DeleteEventType))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Event Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteEventType(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(EventTypeEndpoint.GetEventbyEventType))]
        [OpenApiOperation(tags: _tag, Summary = "Get Event by EventType")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEventbyEventTypeRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventbyEventTypeRequest.IdEventType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<ItemValueVm>))]
        public Task<IActionResult> GetEventbyEventType(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/get-event-by-eventtype")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getEventNamebyEventTypeHandler.Execute(req, cancellationToken);
        }
    }
}
