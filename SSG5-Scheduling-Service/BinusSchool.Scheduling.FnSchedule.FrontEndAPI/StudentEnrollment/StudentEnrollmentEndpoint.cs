using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollment
{
    public class StudentEnrollmentEndpoint
    {
        private const string _route = "schedule/student-enrollment";
        private const string _tag = "Student Enrollment";

        private readonly GetStudentEnrollmentWIthGradeHandler _handlerStudentEnrollmentWIthGradeHandler;

        public StudentEnrollmentEndpoint(GetStudentEnrollmentWIthGradeHandler handlerStudentEnrollmentWIthGradeHandler)
        {
            _handlerStudentEnrollmentWIthGradeHandler = handlerStudentEnrollmentWIthGradeHandler;
        }


        [FunctionName(nameof(StudentEnrollmentEndpoint.GetStudentEnrollmentWithGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student list")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentEnrollmentWithGradeRequest.IdGrade), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(GetStudentEnrollmentWithGradeRequest.IdAscTimetable), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentEnrollmentWithGradeResult))]
        public Task<IActionResult> GetStudentEnrollmentWithGrade(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-student")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handlerStudentEnrollmentWIthGradeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEnrollmentEndpoint.GetStudentEnrollment))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Subject Enrollment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentEnrollmentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentEnrollmentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentEnrollmentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentEnrollmentRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentEnrollmentResult))]
        public Task<IActionResult> GetStudentEnrollment(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentEnrollmentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEnrollmentEndpoint.GetDownloadStudentEnrollment))]
        [OpenApiOperation(tags: _tag, Summary = "Get Download Student Subject Enrollment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentEnrollmentRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentEnrollmentResult))]
        public Task<IActionResult> GetDownloadStudentEnrollment(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDownloadStudentEnrollmentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEnrollmentEndpoint.UpdateStudentEnrollment))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Subject Enrollment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentEnrollmentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentEnrollment(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           [Queue("notification-ays-schedule")] ICollector<string> collector,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateStudentEnrollmentHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(StudentEnrollmentEndpoint.GetStudentEnrollmentStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Subject Enrollment per Student")]
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
        [OpenApiParameter(nameof(GetStudentEnrollmentStudentRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentEnrollmentStudentRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentEnrollmentStudentRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentEnrollmentStudentRequest.IdSubjects), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentEnrollmentStudentResult[]))]
        public Task<IActionResult> GetStudentEnrollmentStudent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentEnrollmentStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }
        
        [FunctionName(nameof(StudentEnrollmentEndpoint.GetStudentEnrollmentHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Subject Enrollment per Homeroom")]
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
        [OpenApiParameter(nameof(GetStudentEnrollmentHomeroomRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentEnrollmentHomeroomRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentEnrollmentHomeroomResult[]))]
        public Task<IActionResult> GetStudentEnrollmentHomeroom(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/homeroom")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentEnrollmentHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }
        [FunctionName(nameof(StudentEnrollmentEndpoint.GetStudentEnrollmentSubject))]
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
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentEnrollmentSubjectResult[]))]
        public Task<IActionResult> GetStudentEnrollmentSubject(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/subject")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentEnrollmentSubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEnrollmentEndpoint.HomeroomStudentEnroolmentDoubelData))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Subject Enrollment per Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(HomeroomStudentEnroolmentDoubelDataRequest.IdAcademicYear), In = ParameterLocation.Query, Required =true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> HomeroomStudentEnroolmentDoubelData(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/double-data")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<HomeroomStudentEnroolmentDoubelDataHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEnrollmentEndpoint.UpdateStudentEnrollmentCopy))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Subject Enrollment Copy Next Semester")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentEnrollmentCopyRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentEnrollmentCopy(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-copy")] HttpRequest req,
           [Queue("notification-ays-schedule")] ICollector<string> collector,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateStudentEnrollmentCopyHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }
    }
}
