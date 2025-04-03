using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom
{
    public class HomeroomEndpoint
    {
        private const string _route = "schedule/homeroom";
        private const string _tag = "Homeroom";

        [FunctionName(nameof(HomeroomEndpoint.GetHomerooms))]
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
        [OpenApiParameter(nameof(GetHomeroomRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHomeroomRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHomeroomRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetHomeroomRequest.IdClassroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHomeroomRequest.IdPathway), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHomeroomRequest.IdVenue), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHomeroomRequest.IdTeacher), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHomeroomResult[]))]
        public Task<IActionResult> GetHomerooms(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(HomeroomEndpoint.GetHomeroomDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homeroom Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHomeroomDetailResult))]
        public Task<IActionResult> GetHomeroomDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHomeroomDetailHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        [FunctionName(nameof(HomeroomEndpoint.AddHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Add Homeroom", Description = @"
            - IdClassroom, IdPathway, IdPathwayDetail: /school-fn-shool/school/classroom-map-by-grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddHomeroomRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddHomeroom(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(HomeroomEndpoint.UpdateHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Update Homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateHomeroomRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateHomeroom(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(HomeroomEndpoint.DeleteHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteHomeroom(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(HomeroomEndpoint.GetHomeroomByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homeroom by Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetHomeroomByStudentRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        [OpenApiParameter(nameof(GetHomeroomByStudentRequest.IdGrades), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHomeroomByStudentResult[]))]
        public Task<IActionResult> GetHomeroomByStudent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-by-student")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<HomeroomByStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(HomeroomEndpoint.AddMoveStudentHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Move Student Grade, Homeroom & Pathway", Description = @"
            IdHomeroom: GET /scheduling-fn-schedule/schedule/homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMoveHomeroomRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddMoveStudentHomeroom(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-move")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MoveHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(HomeroomEndpoint.GetHomeroomsBySubjectTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homerooms By Suject Teacher")]
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
        [OpenApiParameter(nameof(GetHomeroomByTeacherRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHomeroomByTeacherRequest.IdAcademicyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomByTeacherRequest.IdTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomByTeacherRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHomeroomByTeacherResult[]))]
        public Task<IActionResult> GetHomeroomsBySubjectTeacher(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-by-subject-teacher")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHomeroomByTeacherHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(HomeroomEndpoint.GetHomeroomByLevelGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homeroom By Level & Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetHomeroomByLevelGradeRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHomeroomByLevelGradeResult))]
        public Task<IActionResult> GetHomeroomByLevelGrade(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-by-level-grade")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHomeroomByLevelGradeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(HomeroomEndpoint.AddHomeroomCopy))]
        [OpenApiOperation(tags: _tag, Summary = "Add Homeroom Copy")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddHomeroomCopyRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddHomeroomCopy(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-copy")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddHomeroomCopyHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
