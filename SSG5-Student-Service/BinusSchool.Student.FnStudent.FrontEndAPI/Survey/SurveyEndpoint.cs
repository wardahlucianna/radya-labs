using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Student.FnStudent.Survey;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.Survey
{
    public class SurveyEndpoint
    {
        private const string _route = "student/survey";
        private const string _tag = "student Survey";

        private readonly GetSurveyHandler _getSurveyHandler;
        public SurveyEndpoint(GetSurveyHandler getSurveyHandler)
        {
            _getSurveyHandler = getSurveyHandler;
        }

        [FunctionName(nameof(SurveyEndpoint.GetSurvey))]
        [OpenApiOperation(tags: _tag, Summary = "Get Survey List for Blocking Popup")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSurveyRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSurveyRequest.Role), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSurveyRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSurveyResult>))]
        public Task<IActionResult> GetSurvey(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"/GetSurveyList")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getSurveyHandler.Execute(req, cancellationToken);
        }
    }
}
