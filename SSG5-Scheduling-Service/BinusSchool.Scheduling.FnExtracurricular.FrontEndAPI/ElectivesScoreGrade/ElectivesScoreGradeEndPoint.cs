using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectivesScoreGrade;
using BinusSchool.Scheduling.FnExtracurricular.ElectivesScoreGrade;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScoreGrade
{
    public class ElectivesScoreGradeEndPoint
    {
        private const string _route = "elective/elective-score-grade";
        private const string _tag = "Elective Score Grade";

        private readonly ElectivesScoreGradeHandler _electivesScoreGradeHandler;

        public ElectivesScoreGradeEndPoint(
        ElectivesScoreGradeHandler electivesScoreGradeHandler)
        {
            _electivesScoreGradeHandler = electivesScoreGradeHandler;
        }

        [FunctionName(nameof(ElectivesScoreGradeEndPoint.GetElectivesScoreGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Electives Score Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetElectivesScoreGradeRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetElectivesScoreGradeRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetElectivesScoreGradeRequest.IdExtracurricular), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetElectivesScoreGradeResult>))]
        public Task<IActionResult> GetElectivesScoreGrade([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _electivesScoreGradeHandler.Execute(req, cancellationToken);
        }
    }
}
