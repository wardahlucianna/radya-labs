using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Document.FnDocument.BLPSettingPeriod;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.BLPSettingPeriod
{
    public class BLPSettingPeriodEndpoint
    {
        private const string _route = "blp/setting-period";
        private const string _tag = "Blended Learning Program Setting Period";

        private readonly BLPSettingPeriodHandler _bLPSettingPeriodHandler;
        private readonly GetGradeSurveyPeriodHandler _getGradeSurveyPeriodHandler;
        private readonly GetSurveyPeriodByGradeHandler _getSurveyPeriodByGradeHandler;

        public BLPSettingPeriodEndpoint(BLPSettingPeriodHandler bLPSettingPeriodHandler,
             GetGradeSurveyPeriodHandler getGradeSurveyPeriodHandler,
             GetSurveyPeriodByGradeHandler getSurveyPeriodByGradeHandler)
        {
            _bLPSettingPeriodHandler = bLPSettingPeriodHandler;
            _getGradeSurveyPeriodHandler = getGradeSurveyPeriodHandler;
            _getSurveyPeriodByGradeHandler = getSurveyPeriodByGradeHandler;
        }



        [FunctionName(nameof(BLPSettingPeriodEndpoint.GetBLPSettingPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get BLP Setting Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBLPSettingRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBLPSettingRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBLPSettingRequest.IdSurveyCategory), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetBLPSettingResult>))]
        public Task<IActionResult> GetBLPSettingPeriod([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route )] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _bLPSettingPeriodHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(BLPSettingPeriodEndpoint.GetBLPSettingPeriodDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get BLP Setting Period Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBLPSettingDetailResult))]
        public Task<IActionResult> GetBLPSettingPeriodDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            return _bLPSettingPeriodHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(BLPSettingPeriodEndpoint.AddBLPSettingPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Add BLP Setting Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddBLPSettingPeriodRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddBLPSettingPeriod(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _bLPSettingPeriodHandler.Execute(req, cancellationToken, true);
        }

        [FunctionName(nameof(BLPSettingPeriodEndpoint.UpdateBLPSettingPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Update BLPS etting Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateBLPSettingPeriodRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateBLPSettingPeriod(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _bLPSettingPeriodHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(BLPSettingPeriodEndpoint.DeleteBLPSettingPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Delete BLP Setting Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteBLPSettingPeriod(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _bLPSettingPeriodHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BLPSettingPeriodEndpoint.GetGradeSurveyPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade Survey Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradeSurveyPeriodRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeSurveyPeriodRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeSurveyPeriodRequest.IdSurveyCategory), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetGradeSurveyPeriodResult>))]
        public Task<IActionResult> GetGradeSurveyPeriod([HttpTrigger(AuthorizationLevel.Function, "get", Route = "blp/settingperiod/get-grade")] HttpRequest req,
      CancellationToken cancellationToken)
        {
            return _getGradeSurveyPeriodHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BLPSettingPeriodEndpoint.GetSurveyPeriodByGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Survey Period By Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSurveyPeriodByGradeRequest.IdSurveyCategory), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSurveyPeriodByGradeRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSurveyPeriodByGradeResult))]
        public Task<IActionResult> GetSurveyPeriodByGrade([HttpTrigger(AuthorizationLevel.Function, "get", Route = "blp/settingperiod/get-period-by-grade")] HttpRequest req,
      CancellationToken cancellationToken)
        {
            return _getSurveyPeriodByGradeHandler.Execute(req, cancellationToken);
        }
    }
}
