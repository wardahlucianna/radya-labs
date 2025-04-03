using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitSubmission;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitReason;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitForm;
using BinusSchool.Student.FnStudent.StudentExitForm;

namespace BinusSchool.Student.FnStudent.StudentExitSubmission
{
    public class StudentExitSubmissionEndpoint
    {
        private const string _route = "student/student-exit-Submission";
        private const string _tag = "Student Exit Submission";

        //list
        [FunctionName(nameof(StudentExitSubmissionEndpoint.GetListStudentExitSubmission))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentExitSubmissionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExitSubmissionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExitSubmissionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentExitSubmissionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentExitSubmissionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExitSubmissionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExitSubmissionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetStudentExitSubmissionRequest.IdAcademicYear), In = ParameterLocation.Query, Type = typeof(string), Required = true)]
        [OpenApiParameter(nameof(GetStudentExitSubmissionRequest.Semester), In = ParameterLocation.Query, Type = typeof(int?))]
        [OpenApiParameter(nameof(GetStudentExitSubmissionRequest.IdLevel), In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(nameof(GetStudentExitSubmissionRequest.IdGrade), In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(nameof(GetStudentExitSubmissionRequest.IdHomeroom), In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiParameter(nameof(GetStudentExitSubmissionRequest.Status), In = ParameterLocation.Query, Type = typeof(string))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentExitSubmissionResult))]
        public Task<IActionResult> GetListStudentExitSubmission(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<StudentExitSubmissionHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        //update
        [FunctionName(nameof(StudentExitSubmissionEndpoint.UpdateStudentExitSubmission))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentExitSubmissionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentExitSubmission(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            [Queue("notification-student-exit")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<StudentExitSubmissionHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }
    }
}
