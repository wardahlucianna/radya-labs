using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Attendance.FnAttendance.MapAttendance
{
    public class MapAttendanceEndpoint
    {
        private const string _route = "map-attendance";
        private const string _tag = "Mapping Attendance";

        [FunctionName(nameof(MapAttendanceEndpoint.GetMapAttendanceLevels))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMapAttendanceLevelRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetMapAttendanceLevelRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMapAttendanceLevelRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMapAttendanceLevelRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMapAttendanceLevelRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMapAttendanceLevelRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMapAttendanceLevelRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMapAttendanceLevelRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetMapAttendanceLevelRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMapAttendanceLevelResult[]))]
        public Task<IActionResult> GetMapAttendanceLevels(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/level")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMapAttendanceLevelHandler>();
            return handler.Execute(req, cancellationToken);
        }
        
        [FunctionName(nameof(MapAttendanceEndpoint.GetMapAttendanceDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("idLevel", In = ParameterLocation.Path, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMapAttendanceDetailResult))]
        public Task<IActionResult> GetMapAttendanceDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{idLevel}")] HttpRequest req,
            string idLevel,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMapAttendanceDetailHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "idLevel".WithValue(idLevel));
        }

        [FunctionName(nameof(MapAttendanceEndpoint.AddOrUpdateMappingAttendance))]
        [OpenApiOperation(tags: _tag, Summary = "Add Update Mapping Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddOrUpdateMappingAttendanceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddOrUpdateMappingAttendance(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/update")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddOrUpdateMappingAttendanceHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
