using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class MoveStudentEnrollmentEndpoint
    {
        private const string _route = "moving-student/move-student-enrollment";
        private const string _tag = "Move Student Enrollment";

        private readonly MoveStudentEnrollmentHandler _moveStudentEnrollmentHandler;
        public MoveStudentEnrollmentEndpoint(MoveStudentEnrollmentHandler MoveStudentEnrollmentHandler)
        {
            _moveStudentEnrollmentHandler = MoveStudentEnrollmentHandler;
        }

        [FunctionName(nameof(MoveStudentEnrollmentEndpoint.GetMoveStudentEnrollment))]
        [OpenApiOperation(tags: _tag, Summary = "Get Move Student Enrollment")]
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
        [OpenApiParameter(nameof(GetMoveStudentEnrollmentRequest.idAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMoveStudentEnrollmentRequest.semester), In = ParameterLocation.Query, Required = true, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMoveStudentEnrollmentRequest.idLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMoveStudentEnrollmentRequest.idGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMoveStudentEnrollmentRequest.idHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMoveStudentEnrollmentRequest.studentName), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMoveStudentEnrollmentResult[]))]
        public Task<IActionResult> GetMoveStudentEnrollment(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _moveStudentEnrollmentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MoveStudentEnrollmentEndpoint.AddMoveStudentEnrollment))]
        [OpenApiOperation(tags: _tag, Summary = "Add Move Student Enrollment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMoveStudentEnrollmentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddMoveStudentEnrollment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            [Queue("notification-ems")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            return _moveStudentEnrollmentHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MoveStudentEnrollmentEndpoint.GetDetailMoveStudentEnrollment))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Move Student Enrollment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailMoveStudentEnrollmentResult))]
        public Task<IActionResult> GetDetailMoveStudentEnrollment(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
         string id,
        CancellationToken cancellationToken)
        {
            return _moveStudentEnrollmentHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(MoveStudentEnrollmentEndpoint.GetMoveStudentEnrollmentHistory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Programme History")]
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
        [OpenApiParameter(nameof(GetMoveStudentEnrollmentHistoryRequest.idHomeroomStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMoveStudentEnrollmentHistoryResult[]))]
        public Task<IActionResult> GetMoveStudentEnrollmentHistory(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-history")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMoveStudentEnrollmentHistoryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MoveStudentEnrollmentEndpoint.MoveStudentEnrollmentSync))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Programme sync")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(MoveStudentEnrollmentSyncRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> MoveStudentEnrollmentSync(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-sync")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _moveStudentEnrollmentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MoveStudentEnrollmentEndpoint.GetSubjectStudentEnrollment))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Subject Enrollment per Subject")]
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
        [OpenApiParameter(nameof(GetStudentEnrollmentSubjectRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSubjectStudentEnrollmentResult[]))]
        public Task<IActionResult> GetSubjectStudentEnrollment(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-subject")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSubjectStudentEnrollmentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MoveStudentEnrollmentEndpoint.CreateMoveStudentEnrollment))]
        [OpenApiOperation(tags: _tag, Summary = "Create Move Student Enrollment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMoveStudentEnrollmentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<string>[]))]
        public Task<IActionResult> CreateMoveStudentEnrollment(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-add")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CreateMoveStudentEnrollmentHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
