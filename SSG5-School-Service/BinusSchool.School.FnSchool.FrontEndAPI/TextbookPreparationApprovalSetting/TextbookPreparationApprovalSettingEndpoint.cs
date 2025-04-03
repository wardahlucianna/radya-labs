using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApprovalSetting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.TextbookPreparationApprovalSetting
{
    public class TextbookPreparationApprovalSettingEndpoint
    {
        private const string _route = "school/textbook-preparation/approval-setting";
        private const string _tag = "Textbook Preparation Approval Setting";

        private readonly TextbookPreparationApprovalSettingHandler _textbookPreparationApprovalHandler;
        public TextbookPreparationApprovalSettingEndpoint(TextbookPreparationApprovalSettingHandler TextbookPreparationApprovalHandler)
        {
            _textbookPreparationApprovalHandler = TextbookPreparationApprovalHandler;
        }

        [FunctionName(nameof(TextbookPreparationApprovalSettingEndpoint.GetTextbookPreparationApprovalSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get Textbook Preparation Approval Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTextbookPreparationApprovalSettingRequest.IdSchool), In = ParameterLocation.Query,Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTextbookPreparationApprovalSettingResult[]))]
        public Task<IActionResult> GetTextbookPreparationApprovalSetting(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route )] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationApprovalHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(TextbookPreparationApprovalSettingEndpoint.AddTextbookPreparationApprovalSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Add Textbook Preparation Approval Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddTextbookPreparationApprovalSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddTextbookPreparationApprovalSetting(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        [Queue("notification-tp")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _textbookPreparationApprovalHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }
    }
}
