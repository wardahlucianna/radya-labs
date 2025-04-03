using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.StudentPassToPdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Util.FnConverter.StudentPassToPdf
{
    public class StudentPassToPdfEndpoint
    {
        private const string _route = "student-pass-to-pdf";
        private const string _tag = "Student Pass to Pdf";

        [FunctionName(nameof(StudentPassToPdfEndpoint.StudentPassToPdf))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(StudentPassToPdfRequest))]
        [OpenApiResponseWithoutBody(System.Net.HttpStatusCode.OK)]
        public Task<IActionResult> StudentPassToPdf(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<StudentPassToPdfHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}
