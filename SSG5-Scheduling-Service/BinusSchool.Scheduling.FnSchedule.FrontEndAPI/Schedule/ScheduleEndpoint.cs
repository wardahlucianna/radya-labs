using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.Schedule
{
    public class ScheduleEndpoint
    {
        private const string _route = "schedule";
        private const string _tag = "Schedule";

        private readonly GetScheduleHandler _getScheduleHandler;
        private readonly GetScheduleDetailHandler _getScheduleDetailHandler;
        private readonly AddScheduleHandler _addScheduleHandler;
        private readonly UpdateScheduleHandler _updateScheduleHandler;
        private readonly DeleteScheduleHandler _deleteScheduleHandler;

        public ScheduleEndpoint(
            GetScheduleHandler getScheduleHandler,
            GetScheduleDetailHandler getScheduleDetailHandler,
            AddScheduleHandler addScheduleHandler,
            UpdateScheduleHandler updateScheduleHandler,
            DeleteScheduleHandler deleteScheduleHandler)
        {
            _getScheduleHandler = getScheduleHandler;
            _getScheduleDetailHandler = getScheduleDetailHandler;
            _addScheduleHandler = addScheduleHandler;
            _updateScheduleHandler = updateScheduleHandler;
            _deleteScheduleHandler = deleteScheduleHandler;
        }

        [FunctionName(nameof(ScheduleEndpoint.GetSchedules))]
        [OpenApiOperation(tags: _tag, Summary = "Get Schedule List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetScheduleRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetScheduleRequest.IdAscTimetable), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetScheduleRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetScheduleRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetScheduleRequest.Semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ScheduleVm[]))]
        public Task<IActionResult> GetSchedules(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getScheduleHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleEndpoint.GetScheduleDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Schedule Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ScheduleDetailResult))]
        public Task<IActionResult> GetScheduleDetail(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
         string id,
         CancellationToken cancellationToken)
        {
            return _getScheduleDetailHandler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(ScheduleEndpoint.AddSchedule))]
        [OpenApiOperation(tags: _tag, Summary = "Add Schedule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddScheduleRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddSchedule(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _addScheduleHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleEndpoint.UpdateSchedule))]
        [OpenApiOperation(tags: _tag, Summary = "Update Schedule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateScheduleRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateSchedule(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _updateScheduleHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleEndpoint.DeleteSchedule))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Schedule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DeleteScheduleRequest.IdSchedule), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteSchedule(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _deleteScheduleHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ScheduleEndpoint.CheckScheduleByVenue))]
        [OpenApiOperation(tags: _tag, Summary = "Get Schedule List By Venue")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetScheduleByVenueRequest.IdVenue), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(bool[]))]
        public Task<IActionResult> CheckScheduleByVenue(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/venue")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetScheduleByVenueHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
