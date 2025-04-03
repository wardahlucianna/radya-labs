using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Util.FnConverter.BnsReportToPdf;
using BinusSchool.Util.FnConverter.BnsReportToPdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using BinusSchool.Data.Model.Util.FnConverter.MedicalStudentPassToPdf;

namespace BinusSchool.Util.FnConverter.MedicalStudentPassToPdf
{
    public class MedicalStudentPassToPdfEndpoint
    {
        private const string _route = "medical-student-pass-to-pdf";
        private const string _tag = "Medical Student Pass to Pdf";

        private readonly MedicalStudentPassToPdfHandler _medicalStudentPassToPdfHandler;

        public MedicalStudentPassToPdfEndpoint(MedicalStudentPassToPdfHandler medicalStudentPassToPdfHandler)
        {
            _medicalStudentPassToPdfHandler = medicalStudentPassToPdfHandler;
        }

        [FunctionName(nameof(MedicalStudentPassToPdf))]
        [OpenApiOperation(tags: _tag, Summary = "Medical Student Pass To Pdf")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(MedicalStudentPassToPdfRequest))]
        [OpenApiResponseWithBody(System.Net.HttpStatusCode.OK, "application/json", typeof(MedicalStudentPassToPdfResponse))]
        public Task<IActionResult> MedicalStudentPassToPdf(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _medicalStudentPassToPdfHandler.Execute(req, cancellationToken);
        }
    }
}
