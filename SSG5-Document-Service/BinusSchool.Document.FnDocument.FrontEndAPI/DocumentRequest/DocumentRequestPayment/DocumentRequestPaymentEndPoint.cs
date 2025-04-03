using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class DocumentRequestPaymentEndPoint
    {
        private const string _route = "document-request-payment";
        private const string _tag = "Document Request Payment";

        private readonly GetDetailDocumentRequestPaymentConfirmationHandler _getDetailDocumentRequestPaymentConfirmationHandler;
        private readonly AddPaymentConfirmationHandler _addPaymentConfirmationHandler;
        private readonly GetPaymentMethodBySchoolHandler _getPaymentMethodBySchoolHandler;
        private readonly UploadTransferEvidanceDocumentHandler _uploadTransferEvidanceDocumentHandler;
        private readonly GetDocumentRequestPaymentInfoHandler _getDocumentRequestPaymentInfoHandler;
        private readonly SavePaymentApprovalHandler _savePaymentApprovalHandler;
        private readonly ExportExcelDocumentRequestPaymentHandler _exportExcelDocumentRequestPaymentHandler;
        private readonly GetPaymentRecapListHandler _getPaymentRecapListHandler;
        private readonly ExportExcelPaymentRecapListHandler _exportExcelPaymentRecapListHandler;

        public DocumentRequestPaymentEndPoint(
            GetDetailDocumentRequestPaymentConfirmationHandler getDetailDocumentRequestPaymentConfirmationHandler,
            AddPaymentConfirmationHandler addPaymentConfirmationHandler,
            GetPaymentMethodBySchoolHandler getPaymentMethodBySchoolHandler,
            UploadTransferEvidanceDocumentHandler uploadTransferEvidanceDocumentHandler,
            GetDocumentRequestPaymentInfoHandler getDocumentRequestPaymentInfoHandler,
            SavePaymentApprovalHandler savePaymentApprovalHandler,
            ExportExcelDocumentRequestPaymentHandler exportExcelDocumentRequestPaymentHandler,
            GetPaymentRecapListHandler getPaymentRecapListHandler,
            ExportExcelPaymentRecapListHandler exportExcelPaymentRecapListHandler)
        {
            _getDetailDocumentRequestPaymentConfirmationHandler = getDetailDocumentRequestPaymentConfirmationHandler;
            _addPaymentConfirmationHandler = addPaymentConfirmationHandler;
            _getPaymentMethodBySchoolHandler = getPaymentMethodBySchoolHandler;
            _uploadTransferEvidanceDocumentHandler = uploadTransferEvidanceDocumentHandler;
            _getDocumentRequestPaymentInfoHandler = getDocumentRequestPaymentInfoHandler;
            _savePaymentApprovalHandler = savePaymentApprovalHandler;
            _exportExcelDocumentRequestPaymentHandler = exportExcelDocumentRequestPaymentHandler;
            _getPaymentRecapListHandler = getPaymentRecapListHandler;
            _exportExcelPaymentRecapListHandler = exportExcelPaymentRecapListHandler;
        }

        [FunctionName(nameof(DocumentRequestPaymentEndPoint.GetDetailDocumentRequestPaymentConfirmation))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Document Request Payment Confirmation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailDocumentRequestPaymentConfirmationRequest.IdDocumentReqApplicant), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailDocumentRequestPaymentConfirmationResult))]
        public Task<IActionResult> GetDetailDocumentRequestPaymentConfirmation(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-document-request-payment-confirmation")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getDetailDocumentRequestPaymentConfirmationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestPaymentEndPoint.AddPaymentConfirmation))]
        [OpenApiOperation(tags: _tag, Summary = "Add Payment Confirmation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(CreateDocumentRequestStaffRequest))]
        //[OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddPaymentConfirmationRequest))]
        public Task<IActionResult> AddPaymentConfirmation(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-payment-confirmation")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _addPaymentConfirmationHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(DocumentRequestPaymentEndPoint.GetPaymentMethodBySchool))]
        [OpenApiOperation(tags: _tag, Summary = "Get Payment Method by School")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPaymentMethodBySchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPaymentMethodBySchoolRequest.IdDocumentReqPaymentMethod), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPaymentMethodBySchoolRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPaymentMethodBySchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPaymentMethodBySchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetPaymentMethodBySchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetPaymentMethodBySchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPaymentMethodBySchoolRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPaymentMethodBySchoolRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetPaymentMethodBySchoolResult>))]
        public Task<IActionResult> GetPaymentMethodBySchool(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-payment-method-by-school")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getPaymentMethodBySchoolHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestPaymentEndPoint.UploadTransferEvidanceDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Upload Transfer Evidance Document")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(CreateDocumentRequestStaffRequest))]
        //[OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UploadTransferEvidanceDocumentResult))]
        public Task<IActionResult> UploadTransferEvidanceDocument(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/upload-transfer-evidance-document")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _uploadTransferEvidanceDocumentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestPaymentEndPoint.GetDocumentRequestPaymentInfo))]
        [OpenApiOperation(tags: _tag, Summary = "Get Document Request Payment Info")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentRequestPaymentInfoRequest.IdDocumentReqApplicantList), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetDocumentRequestPaymentInfoResult>))]
        public Task<IActionResult> GetDocumentRequestPaymentInfo(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-document-request-payment-info")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getDocumentRequestPaymentInfoHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestPaymentEndPoint.SavePaymentApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Save Payment Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(CreateDocumentRequestStaffRequest))]
        //[OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SavePaymentApprovalRequest))]
        public Task<IActionResult> SavePaymentApproval(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-payment-approval")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _savePaymentApprovalHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestPaymentEndPoint.ExportExcelDocumentRequestPayment))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Document Request Payment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelDocumentRequestPaymentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelDocumentRequestPayment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-document-request-payment-excel")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _exportExcelDocumentRequestPaymentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestPaymentEndPoint.GetPaymentRecapList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Payment Recap List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPaymentRecapListRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPaymentRecapListRequest.PaymentPeriodStartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPaymentRecapListRequest.PaymentPeriodEndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetPaymentRecapListResult>))]
        public Task<IActionResult> GetPaymentRecapList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-payment-recap-list")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getPaymentRecapListHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(DocumentRequestPaymentEndPoint.ExportExcelPaymentRecapList))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Payment Recap List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelPaymentRecapListRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelPaymentRecapList(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-payment-recap-list-excel")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _exportExcelPaymentRecapListHandler.Execute(req, cancellationToken);
        }
    }
}
