using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.Document;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.Document
{
    public class DocumentEndpoint
    {
        private const string _route = "document/doc";
        private const string _tag = "Form Builder Document";

        private readonly DocumentHandler _handler;
        private readonly DocumentApprovalHandler _approvalhandler;
        private readonly DocumentValueHandler _documentValueHandler;

        public DocumentEndpoint(DocumentHandler handler, DocumentApprovalHandler approvalhandler, DocumentValueHandler documentValueHandler)
        {
            _handler = handler;
            _approvalhandler = approvalhandler;
            _documentValueHandler = documentValueHandler;
        }

        [FunctionName(nameof(DocumentEndpoint.GetDocuments))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDocumentRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDocumentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDocumentRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequest.IdSubject), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequest.IdTerm), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDocumentResult))]
        public Task<IActionResult> GetDocuments(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentEndpoint.GetDocumentDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDocumentDetailResult))]
        public Task<IActionResult> GetDocumentDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }


        [FunctionName(nameof(DocumentEndpoint.UpdateDocument))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateDocumentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateDocument(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentEndpoint.ApprovalDocument))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ApprovalDocumentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ApprovalDocument(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-approval")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _approvalhandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentEndpoint.GetDocumentValues))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(SelectDocumentValueRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(SelectDocumentValueRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(SelectDocumentValueRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(SelectDocumentValueRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(SelectDocumentValueRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(SelectDocumentValueRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(SelectDocumentValueRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(SelectDocumentValueRequest.IdDocument), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(SelectDocumentValueRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetDocumentValues(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-value")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _documentValueHandler.Execute(req, cancellationToken);
        }
    }
}
