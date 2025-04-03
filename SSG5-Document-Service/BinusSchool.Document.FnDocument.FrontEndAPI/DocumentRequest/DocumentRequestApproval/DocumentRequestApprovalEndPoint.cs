using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApproval;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApproval
{
    public class DocumentRequestApprovalEndPoint
    {
        private const string _route = "document-request-approval";
        private const string _tag = "Document Request Approval";

        private readonly SaveDocumentRequestApprovalHandler _saveDocumentRequestApprovalHandler;

        public DocumentRequestApprovalEndPoint(SaveDocumentRequestApprovalHandler saveDocumentRequestApprovalHandler)
        {
            _saveDocumentRequestApprovalHandler = saveDocumentRequestApprovalHandler;
        }

        [FunctionName(nameof(DocumentRequestApprovalEndPoint.SaveDocumentRequestApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Save Document Request Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveDocumentRequestApprovalRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveDocumentRequestApproval(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-document-request-approval")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveDocumentRequestApprovalHandler.Execute(req, cancellationToken);
        }
    }
}
