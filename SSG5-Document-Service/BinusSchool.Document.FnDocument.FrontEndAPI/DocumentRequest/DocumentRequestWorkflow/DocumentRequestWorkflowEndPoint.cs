using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow
{
    public class DocumentRequestWorkflowEndPoint
    {
        private const string _route = "document-request-status-workflow";
        private const string _tag = "Document Request Status Workflow";

        private readonly AddDocumentRequestWorkflowHandler _addDocumentRequestWorkflowHandler;
        private readonly GetDocumentRequestStatusWorkflowListHandler _getDocumentRequestStatusWorkflowListHandler;
        private readonly GetDateBusinessDaysByStartDateHandler _getDateBusinessDaysByStartDateHandler;

        public DocumentRequestWorkflowEndPoint(
            AddDocumentRequestWorkflowHandler addDocumentRequestWorkflowHandler,
            GetDocumentRequestStatusWorkflowListHandler getDocumentRequestStatusWorkflowListHandler,
            GetDateBusinessDaysByStartDateHandler getDateBusinessDaysByStartDateHandler)
        {
            _addDocumentRequestWorkflowHandler = addDocumentRequestWorkflowHandler;
            _getDocumentRequestStatusWorkflowListHandler = getDocumentRequestStatusWorkflowListHandler;
            _getDateBusinessDaysByStartDateHandler = getDateBusinessDaysByStartDateHandler;
        }

        [FunctionName(nameof(DocumentRequestWorkflowEndPoint.AddDocumentRequestWorkflow))]
        [OpenApiOperation(tags: _tag, Summary = "Add Document Request Workflow")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddDocumentRequestWorkflowRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddDocumentRequestWorkflowResult))]
        public Task<IActionResult> AddDocumentRequestWorkflow(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-document-request-workflow")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _addDocumentRequestWorkflowHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestWorkflowEndPoint.GetDocumentRequestStatusWorkflowList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Document Request Status Workflow List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentRequestStatusWorkflowListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestStatusWorkflowListRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestStatusWorkflowListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDocumentRequestStatusWorkflowListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDocumentRequestStatusWorkflowListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestStatusWorkflowListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestStatusWorkflowListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDocumentRequestStatusWorkflowListRequest.IsFromParent), In = ParameterLocation.Query, Type = typeof(bool), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetDocumentRequestStatusWorkflowListResult>))]
        public Task<IActionResult> GetDocumentRequestStatusWorkflowList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-document-request-status-workflow-list")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDocumentRequestStatusWorkflowListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestWorkflowEndPoint.GetDateBusinessDaysByStartDate))]
        [OpenApiOperation(tags: _tag, Summary = "Get Date Business Days by Start Date")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDateBusinessDaysByStartDateRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDateBusinessDaysByStartDateRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDateBusinessDaysByStartDateRequest.TotalDays), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDateBusinessDaysByStartDateRequest.CountHoliday), In = ParameterLocation.Query, Type = typeof(bool), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDateBusinessDaysByStartDateResult))]
        public Task<IActionResult> GetDateBusinessDaysByStartDate(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-date-business-days-by-start-date")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDateBusinessDaysByStartDateHandler.Execute(req, cancellationToken);
        }
    }
}
