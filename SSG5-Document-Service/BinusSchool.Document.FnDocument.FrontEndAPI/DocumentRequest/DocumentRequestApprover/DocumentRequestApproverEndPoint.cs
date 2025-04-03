using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestApprover
{
    public class DocumentRequestApproverEndPoint
    {
        private const string _route = "document-request-approver";
        private const string _tag = "Document Request Approver";

        private readonly GetApproverListBySchoolHandler _getApproverListBySchoolHandler;
        private readonly GetUnmappedApproverStaffListHandler _getUnmappedApproverStaffListHandler;
        private readonly AddDocumentRequestApproverHandler _addDocumentRequestApproverHandler;
        private readonly RemoveDocumentRequestApproverHandler _removeDocumentRequestApproverHandler;
        private readonly CheckAdminAccessByIdBinusianHandler _checkAdminAccessByIdBinusianHandler;

        public DocumentRequestApproverEndPoint(
            GetApproverListBySchoolHandler getApproverListBySchoolHandler,
            GetUnmappedApproverStaffListHandler getUnmappedApproverStaffListHandler,
            AddDocumentRequestApproverHandler addDocumentRequestApproverHandler,
            RemoveDocumentRequestApproverHandler removeDocumentRequestApproverHandler,
            CheckAdminAccessByIdBinusianHandler checkAdminAccessByIdBinusianHandler
            )
        {
            _getApproverListBySchoolHandler = getApproverListBySchoolHandler;
            _getUnmappedApproverStaffListHandler = getUnmappedApproverStaffListHandler;
            _addDocumentRequestApproverHandler = addDocumentRequestApproverHandler;
            _removeDocumentRequestApproverHandler = removeDocumentRequestApproverHandler;
            _checkAdminAccessByIdBinusianHandler = checkAdminAccessByIdBinusianHandler;
        }

        [FunctionName(nameof(DocumentRequestApproverEndPoint.GetApproverListBySchool))]
        [OpenApiOperation(tags: _tag, Summary = "Get Approver List By School")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetApproverListBySchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetApproverListBySchoolRequest.IdBinusian), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetApproverListBySchoolResult>))]
        public Task<IActionResult> GetApproverListBySchool(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-approver-list-by-school")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getApproverListBySchoolHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestApproverEndPoint.GetUnmappedApproverStaffList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Unmapped Approver Staff List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnmappedApproverStaffListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnmappedApproverStaffListRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnmappedApproverStaffListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnmappedApproverStaffListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnmappedApproverStaffListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnmappedApproverStaffListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnmappedApproverStaffListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUnmappedApproverStaffListRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnmappedApproverStaffListResult))]
        public Task<IActionResult> GetUnmappedApproverStaffList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-unmapped-approver-staff-list")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getUnmappedApproverStaffListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestApproverEndPoint.AddDocumentRequestApprover))]
        [OpenApiOperation(tags: _tag, Summary = "Add Document Request Approver")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddDocumentRequestApproverRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddDocumentRequestApprover(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-document-request-approver")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _addDocumentRequestApproverHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestApproverEndPoint.RemoveDocumentRequestApprover))]
        [OpenApiOperation(tags: _tag, Summary = "Remove Document Request Approver")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(RemoveDocumentRequestApproverRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> RemoveDocumentRequestApprover(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/remove-document-request-approver")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _removeDocumentRequestApproverHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestApproverEndPoint.CheckAdminAccessByIdBinusian))]
        [OpenApiOperation(tags: _tag, Summary = "Check Admin Access by IdBinusian")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CheckAdminAccessByIdBinusianRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(CheckAdminAccessByIdBinusianRequest.IdBinusian), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CheckAdminAccessByIdBinusianResult))]
        public Task<IActionResult> CheckAdminAccessByIdBinusian(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/check-admin-access-by-idbinusian")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _checkAdminAccessByIdBinusianHandler.Execute(req, cancellationToken);
        }
    }
}
