using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentSubject
{
    public class MovingStudentSubjectEndpoint
    {
        private const string _route = "moving-student/move-student-subject";
        private const string _tag = "Move Student Subject";

        public MovingStudentSubjectEndpoint()
        {
        }

        [FunctionName(nameof(MovingStudentSubjectEndpoint.GetHomeroomMoveStudentSubject))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homeroom Move Student Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetHomeroomMoveStudentSubjectRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomMoveStudentSubjectRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomMoveStudentSubjectRequest.IdGradeOld), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHomeroomMoveStudentSubjectResult[]))]
        public Task<IActionResult> GetHomeroomMoveStudentSubject(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/homeroom")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHomeroomMoveStudentSubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MovingStudentSubjectEndpoint.GetSubjectMoveStudentSubject))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject Move Student Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubjectMoveStudentSubjectRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectMoveStudentSubjectRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectMoveStudentSubjectRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectMoveStudentSubjectRequest.IdHomeroomOld), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubjectMoveStudentSubjectRequest.IdLessonOld), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSubjectMoveStudentSubjectResult[]))]
        public Task<IActionResult> GetSubjectMoveStudentSubject(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/subject")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSubjectMoveStudentSubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MovingStudentSubjectEndpoint.GetStudentMoveStudentSubject))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Move Student Subject")]
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
        [OpenApiParameter(nameof(GetStudentMoveStudentSubjectRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentMoveStudentSubjectRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentMoveStudentSubjectRequest.IdLessonOld), In = ParameterLocation.Query,Required =true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentMoveStudentSubjectResult[]))]
        public Task<IActionResult> GetStudentMoveStudentSubject(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentMoveStudentSubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MovingStudentSubjectEndpoint.AddMoveStudentSubject))]
        [OpenApiOperation(tags: _tag, Summary = "Add Move Student Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMoveStudentSubjectRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddMoveStudentSubject(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddMoveStudentSubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }


}
