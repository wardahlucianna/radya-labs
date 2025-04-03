using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularSummary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularSummary
{
    public class ExtracurricularSummaryEndPoint
    {
        private const string _route = "extracurricular-summary";
        private const string _tag = "Extracurricular Summary";

        private readonly GetExtracurricularSummaryByStudentHandler _getExtracurricularSummaryByStudentHandler;

        public ExtracurricularSummaryEndPoint(
            GetExtracurricularSummaryByStudentHandler getExtracurricularSummaryByStudentHandler)
        {
            _getExtracurricularSummaryByStudentHandler = getExtracurricularSummaryByStudentHandler;
        }

        [FunctionName(nameof(ExtracurricularSummaryEndPoint.GetExtracurricularSummaryByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Summary By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetExtracurricularSummaryByStudentRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetExtracurricularSummaryByStudentRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetExtracurricularSummaryByStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetExtracurricularSummaryByStudentResult))]
        public Task<IActionResult> GetExtracurricularSummaryByStudent([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-extracurricular-summary-by-student")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _getExtracurricularSummaryByStudentHandler.Execute(req, cancellationToken);
        }
    }
}
