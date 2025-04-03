using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Student.FnStudent.StudentApprovalSummary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.StudentApprovalSummary
{
    public class GetStudentApprovalSummaryEndPoint
    {
        private const string _route = "student/StudentApprovalSummary";
        private const string _tag = "StudentApprovalSummary";

        [FunctionName(nameof(GetStudentApprovalSummaryEndPoint.GetStudentApprovalSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get document Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentApprovalSummaryRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentApprovalSummaryResult))]
        public Task<IActionResult> GetStudentApprovalSummary(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/GetStudentApprovalSummary")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentApprovalSummaryHandler>();
            return handler.Execute(req, cancellationToken,false);
        }
    }
}
