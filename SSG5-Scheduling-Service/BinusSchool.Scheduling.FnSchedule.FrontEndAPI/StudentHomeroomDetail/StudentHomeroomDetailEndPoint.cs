using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentHomeroomDetail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.StudentHomeroomDetail
{
    public class StudentHomeroomDetailEndPoint
    {
        private const string _route = "schedule/student-homeroom-detail";
        private const string _tag = "Student Homeroom Detail";

        [FunctionName(nameof(StudentHomeroomDetailEndPoint.GetHomeroomByStudentId))]
        [OpenApiOperation(tags: _tag, Summary = "Student Homeroom Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentHomeroomDetailRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentHomeroomDetailResult))]
        public Task<IActionResult> GetHomeroomByStudentId(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<HomeroomByStudentIdHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentHomeroomDetailEndPoint.GetGradeAndClassByStudents))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade And Class By Students")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetHomeroomByStudentRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        [OpenApiParameter(nameof(GetHomeroomByStudentRequest.IdGrades), In = ParameterLocation.Query, Type = typeof(string[]), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeAndClassByStudentResult[]))]
        public Task<IActionResult> GetGradeAndClassByStudents(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/get-grade-and-class-student")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGradeAndClassByStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentHomeroomDetailEndPoint.GetHomeroomStudentByLevelGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homeroom Student By Level & Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetHomeroomStudentByLevelGradeRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetHomeroomStudentByLevelGradeResult>))]
        public Task<IActionResult> GetHomeroomStudentByLevelGrade(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "schedule/get-homeroom-student-by-level-grade")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHomeroomStudentByLevelGradeHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
