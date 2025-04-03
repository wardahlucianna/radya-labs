using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularHistory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularHistory
{
    public class ExtracurricularHistoryEndPoint
    {
        private const string _route = "extracurricular-history";
        private const string _tag = "Extracurricular History";

        private readonly GetStudentExtracurricularHistoryListHandler _getStudentExtracurricularHistoryListHandler;

        public ExtracurricularHistoryEndPoint(
            GetStudentExtracurricularHistoryListHandler getStudentExtracurricularHistoryListHandler)
        {
            _getStudentExtracurricularHistoryListHandler = getStudentExtracurricularHistoryListHandler;
        }

        [FunctionName(nameof(ExtracurricularHistoryEndPoint.GetStudentExtracurricularHistoryList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Extracurricular History List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentExtracurricularHistoryListRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentExtracurricularHistoryListRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentExtracurricularHistoryListRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentExtracurricularHistoryListResult>))]
        public Task<IActionResult> GetStudentExtracurricularHistoryList(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-extracurricular-history-list")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _getStudentExtracurricularHistoryListHandler.Execute(req, cancellationToken);
        }
    }
}
