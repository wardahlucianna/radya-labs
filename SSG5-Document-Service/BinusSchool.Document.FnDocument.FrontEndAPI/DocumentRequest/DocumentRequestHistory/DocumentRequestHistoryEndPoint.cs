using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestHistory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestHistory
{
    public class DocumentRequestHistoryEndPoint
    {
        private const string _route = "document-request-history";
        private const string _tag = "Document Request History";

        private readonly GetListDocumentRequestYearHandler _getListDocumentRequestYearHandler;
        private readonly GetDocumentRequestHistoryByStudentHandler _getDocumentRequestHistoryByStudentHandler;
        private readonly CancelDocumentRequestByParentHandler _cancelDocumentRequestByParentHandler;

        public DocumentRequestHistoryEndPoint(
            GetListDocumentRequestYearHandler getListDocumentRequestYearHandler,
            GetDocumentRequestHistoryByStudentHandler getDocumentRequestHistoryByStudentHandler,
            CancelDocumentRequestByParentHandler cancelDocumentRequestByParentHandler)
        {
            _getListDocumentRequestYearHandler = getListDocumentRequestYearHandler;
            _getDocumentRequestHistoryByStudentHandler = getDocumentRequestHistoryByStudentHandler;
            _cancelDocumentRequestByParentHandler = cancelDocumentRequestByParentHandler;
        }

        [FunctionName(nameof(DocumentRequestHistoryEndPoint.GetListDocumentRequestYear))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Document Request Year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListDocumentRequestYearRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListDocumentRequestYearRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListDocumentRequestYearRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListDocumentRequestYearRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListDocumentRequestYearRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListDocumentRequestYearRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListDocumentRequestYearRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListDocumentRequestYearRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListDocumentRequestYearRequest.IdParent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListDocumentRequestYearRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListDocumentRequestYearResult>))]
        public Task<IActionResult> GetListDocumentRequestYear(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-document-request-year")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListDocumentRequestYearHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestHistoryEndPoint.GetDocumentRequestHistoryByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Document Request History by Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentRequestHistoryByStudentRequest.IdParent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDocumentRequestHistoryByStudentRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestHistoryByStudentRequest.IdDocumentReqStatusWorkflow), In = ParameterLocation.Query, Type = typeof(int?))]
        [OpenApiParameter(nameof(GetDocumentRequestHistoryByStudentRequest.RequestYear), In = ParameterLocation.Query, Type = typeof(int?))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetDocumentRequestHistoryByStudentResult>))]
        public Task<IActionResult> GetDocumentRequestHistoryByStudent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-document-request-history-by-student")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDocumentRequestHistoryByStudentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestHistoryEndPoint.CancelDocumentRequestByParent))]
        [OpenApiOperation(tags: _tag, Summary = "Cancel Document Request By Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CancelDocumentRequestByParentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CancelDocumentRequestByParent(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/cancel-document-request-by-parent")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _cancelDocumentRequestByParentHandler.Execute(req, cancellationToken);
        }
    }
}
