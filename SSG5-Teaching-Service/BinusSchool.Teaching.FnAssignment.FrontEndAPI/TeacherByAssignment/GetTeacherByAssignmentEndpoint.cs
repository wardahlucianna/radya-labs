using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherByAssignment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;


namespace BinusSchool.Teaching.FnAssignment.TeacherByAssignment
{
    public class GetTeacherByAssignmentEndpoint
    {

        private const string _route = "teaching/TeacherByAssignment";
        private const string _tag = "TeacherByAssignment";

        [FunctionName(nameof(GetTeacherByAssignmentEndpoint.GetTeacherByDepartment))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher By Department")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetTeacherByDepartmentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherByDepartmentResult))]
        public Task<IActionResult> GetTeacherByDepartment(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetTeacherByDepartment")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherByDepartmentHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(GetTeacherByAssignmentEndpoint.GetTeacherByGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher By Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetTeacherByGradeRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTeacherByGradeResult))]
        public Task<IActionResult> GetTeacherByGrade(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetTeacherByGrade")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherByGradeHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

    }
}
