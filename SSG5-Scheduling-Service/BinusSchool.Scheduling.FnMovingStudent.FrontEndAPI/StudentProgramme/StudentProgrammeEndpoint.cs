using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnMovingStudent.StudentProgramme
{
    public class StudentProgrammeEndpoint
    {
        private const string _route = "moving-student/student-programme";
        private const string _tag = "Student Programme";

        private readonly DeleteStudentProgrammeHandler _deleteStudentProgrammeHandler;
        private readonly StudentProgrammeHandler _studentProgrammeHandler;
        public StudentProgrammeEndpoint(StudentProgrammeHandler StudentProgrammeHandler, DeleteStudentProgrammeHandler DeleteStudentProgrammeHandler)
        {
            _studentProgrammeHandler = StudentProgrammeHandler;
            _deleteStudentProgrammeHandler = DeleteStudentProgrammeHandler;
        }

        [FunctionName(nameof(StudentProgrammeEndpoint.GetStudentProgramme))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Programme")]
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
        [OpenApiParameter(nameof(GetStudentProgrammeRequest.idAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentProgrammeRequest.programme), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentProgrammeResult[]))]
        public Task<IActionResult> GetStudentProgramme(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _studentProgrammeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentProgrammeEndpoint.AddStudentProgramme))]
        [OpenApiOperation(tags: _tag, Summary = "Add Student Programme")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddStudentProgrammeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddStudentProgramme(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _studentProgrammeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentProgrammeEndpoint.GetDetailStudentProgramme))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Student Programme")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailStudentProgrammeResult))]
        public Task<IActionResult> GetDetailStudentProgramme(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
         string id,
        CancellationToken cancellationToken)
        {
            return _studentProgrammeHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(StudentProgrammeEndpoint.UpdateStudentProgramme))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Programme")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentProgrammeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateStudentProgramme(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            [Queue("notification-esp")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            return _studentProgrammeHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(StudentProgrammeEndpoint.DeleteStudentProgramme))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Student Programme")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteStudentProgrammeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteStudentProgramme(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        [Queue("notification-esp")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _deleteStudentProgrammeHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(StudentProgrammeEndpoint.GetStudentProgrammeHistory))]
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
        [OpenApiParameter(nameof(GetStudentProgrammeHistoryRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentProgrammeHistoryResult[]))]
        public Task<IActionResult> GetStudentProgrammeHistory(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-history")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentProgrammeHistoryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentProgrammeEndpoint.GetStudentProgrammeStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Programme")]
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
        [OpenApiParameter(nameof(GetStudentProgrammeStudentRequest.schoolId), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentProgrammeStudentResult[]))]
        public Task<IActionResult> GetStudentProgrammeStudent(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-student")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentProgrammeStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
