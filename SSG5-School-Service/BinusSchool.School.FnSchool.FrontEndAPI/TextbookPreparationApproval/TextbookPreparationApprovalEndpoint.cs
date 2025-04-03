using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApproval;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.TextbookPreparationApproval
{
    public class TextbookPreparationApprovalEndpoint
    {
        private const string _route = "school/textbook-preparation/approval";
        private const string _tag = "Textbook Preparation Approval";

        private readonly TextbookPreparationApprovalHandler _textbookPreparationApprovalHandler;
        public TextbookPreparationApprovalEndpoint(TextbookPreparationApprovalHandler TextbookPreparationApprovalHandler)
        {
            _textbookPreparationApprovalHandler = TextbookPreparationApprovalHandler;
        }

        [FunctionName(nameof(TextbookPreparationApprovalEndpoint.GetTextbookPreparationApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Get Textbook Preparation Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTextbookPreparationApprovalRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTextbookPreparationApprovalRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTextbookPreparationApprovalRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationApprovalRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationApprovalRequest.Status), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationApprovalRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationApprovalRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationApprovalRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTextbookPreparationApprovalRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetTextbookPreparationApprovalRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTextbookPreparationApprovalRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTextbookPreparationApprovalResult[]))]
        public Task<IActionResult> GetTextbookPreparationApproval(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationApprovalHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TextbookPreparationApprovalEndpoint.TextbookPreparationApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Textbook Preparation Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(TextbookPreparationApprovalRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> TextbookPreparationApproval(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        [Queue("notification-tp")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationApprovalHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(TextbookPreparationApprovalEndpoint.DownloadTextbookPreparationApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Download Textbook Preparation Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadTextbookPreparationApprovalRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadTextbookPreparationApprovalRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadTextbookPreparationApprovalRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadTextbookPreparationApprovalRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadTextbookPreparationApprovalRequest.Status), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadTextbookPreparationApprovalRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DownloadTextbookPreparationApproval(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-download")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadTextbookPreparationApprovalHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}
