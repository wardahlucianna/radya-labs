using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.BlendedLearningProgram
{
    public class BLPEndpoint
    {
        private const string _route = "document/blp";
        private const string _tag = "Blended Learning Program";

        private readonly GetBLPQuestionHandler _getBLPQuestionHandler;
        private readonly GetBLPQuestionWithHistoryHandler _getBLPQuestionWithHistoryHandler;
        public BLPEndpoint(GetBLPQuestionHandler getBLPQuestionHandler,
            GetBLPQuestionWithHistoryHandler getBLPQuestionWithHistoryHandler)
        {
            _getBLPQuestionHandler = getBLPQuestionHandler;
            _getBLPQuestionWithHistoryHandler = getBLPQuestionWithHistoryHandler;
        }

        [FunctionName(nameof(BLPEndpoint.GetBLPQuestion))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBLPQuestionRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBLPQuestionRequest.IdSurveyCategory), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBLPQuestionRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBLPQuestionRequest.IdSurveyPeriod), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBLPQuestionRequest.IdClearanceWeekPeriod), In = ParameterLocation.Query)]

        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetBLPQuestionResult>))]
        public Task<IActionResult> GetBLPQuestion(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-question")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getBLPQuestionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BLPEndpoint.GetBLPQuestionWithHistory))]
        [OpenApiOperation(tags: _tag, Summary = "Get BLP Question With History")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetBLPQuestionWithHistoryRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBLPQuestionWithHistoryRequest.IdSurveyCategory), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetBLPQuestionWithHistoryRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBLPQuestionWithHistoryRequest.IdSurveyPeriod), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBLPQuestionWithHistoryRequest.IdClearanceWeekPeriod), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetBLPQuestionWithHistoryResult>))]
        public Task<IActionResult> GetBLPQuestionWithHistory(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-question-with-history")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getBLPQuestionWithHistoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BLPEndpoint.SaveRespondAnswerBLPHandler))]
        [OpenApiOperation(tags: _tag, Summary = "Save Respond Answer BLP")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveRespondAnswerBLPRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveRespondAnswerBLPHandler(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-save-respond-answer")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SaveRespondAnswerBLPHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(BLPEndpoint.UploadFileBLP))]
        [OpenApiOperation(tags: _tag, Summary = "Upload File BLP")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UploadFileBLPRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UploadFileBLPRResult))]
        public Task<IActionResult> UploadFileBLP(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-upload-file")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UploadFileBLPHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
