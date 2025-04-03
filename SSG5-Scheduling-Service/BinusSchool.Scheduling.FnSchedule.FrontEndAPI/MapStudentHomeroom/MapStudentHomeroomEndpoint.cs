using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.MapStudentHomeroom;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.MapStudentHomeroom
{
    public class MapStudentHomeroomEndpoint
    {
        private const string _route = "schedule/map-student-homeroom";
        private const string _tag = "Mapping Student to Homeroom";

        [FunctionName(nameof(MapStudentHomeroomEndpoint.AddMapStudentHomeroom))]
        [OpenApiOperation(tags: _tag, Description = @"
            - IdHomeroom: GET /scheduling-fn-schedule/schedule/homeroom
            - IdStudent, Gender, Religion: GET /student-fn-student/student-by-grade?includePathway=true")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMapStudentHomeroomRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddMapStudentHomeroom(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MapStudentHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MapStudentHomeroomEndpoint.GetMapStudentHomeroomDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homeroom Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true, Description = "Pick from IdHomeroom")]
        [OpenApiParameter(nameof(GetMapStudentHomeroomDetailRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMapStudentHomeroomDetailRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMapStudentHomeroomDetailRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMapStudentHomeroomDetailResult[]))]
        public Task<IActionResult> GetMapStudentHomeroomDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMapStudentHomeroomDetailHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(MapStudentHomeroomEndpoint.DeleteMapStudentHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(string[]), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteMapStudentHomeroom(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MapStudentHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MapStudentHomeroomEndpoint.GetAvailableMapStudentHomerooms))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homeroom List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetMapStudentHomeroomAvailableRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMapStudentHomeroomAvailableRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMapStudentHomeroomAvailableRequest.ExceptIds), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMapStudentPathwayResult[]))]
        public Task<IActionResult> GetAvailableMapStudentHomerooms(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/available")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MapStudentHomeroomAvailableHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MapStudentHomeroomEndpoint.GetHomeroomCopyToNextOrPreviousSemester))]
        [OpenApiOperation(tags: _tag, Summary = "Copy student homeroom to next/previous semester")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetHomeroomCopyToNextOrPreviousSemesterRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHomeroomCopyToNextOrPreviousSemesterResult))]
        public Task<IActionResult> GetHomeroomCopyToNextOrPreviousSemester(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/copy-to-next-or-previous")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHomeroomCopyToNextOrPreviousSemesterHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
