using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationUserPeriod;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.TextbookPreparationUserPeriod
{
    public class TextbookPreparationUserPeriodEndPoint
    {
        private const string _route = "school/textbook-preparation/user-period";
        private const string _tag = "Textbook Preparation User Period";

        private readonly TextbookPreparationUserPeriodHandler _textbookPreparationUserPeriodHandler;
        public TextbookPreparationUserPeriodEndPoint(TextbookPreparationUserPeriodHandler TextbookPreparationUserPeriodHandler)
        {
            _textbookPreparationUserPeriodHandler = TextbookPreparationUserPeriodHandler;
        }

        [FunctionName(nameof(TextbookPreparationUserPeriodEndPoint.GetTextbookPreparationUserPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Textbook Preparation User Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTextbookPreparationUserPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTextbookPreparationUserPeriodRequest.AssignAs), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationUserPeriodRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationUserPeriodRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationUserPeriodRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTextbookPreparationUserPeriodRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTextbookPreparationUserPeriodRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationUserPeriodRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTextbookPreparationUserPeriodResult[]))]
        public Task<IActionResult> GetTextbookPreparationUserPeriod(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationUserPeriodHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TextbookPreparationUserPeriodEndPoint.AddTextbookPreparationUserPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Add Textbook Preparation User Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddTextbookPreparationUserPeriodRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddTextbookPreparationUserPeriod(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        [Queue("notification-tp")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationUserPeriodHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(TextbookPreparationUserPeriodEndPoint.DetailTextbookPreparationUserPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Textbook Preparation User Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailTextbookPreparationUserPeriodResult))]
        public Task<IActionResult> DetailTextbookPreparationUserPeriod(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
          string id,
       CancellationToken cancellationToken)
        {
            return _textbookPreparationUserPeriodHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(TextbookPreparationUserPeriodEndPoint.UpdateTextbookPreparationUserPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Update Textbook Preparation User Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateTextbookPreparationUserPeriodRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateTextbookPreparationUserPeriod(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        [Queue("notification-tp")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationUserPeriodHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(TextbookPreparationUserPeriodEndPoint.DeleteTextbookPreparationUserPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Textbook Preparation User Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteTextbookPreparationUserPeriod(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationUserPeriodHandler.Execute(req, cancellationToken);
        }
    }
}
