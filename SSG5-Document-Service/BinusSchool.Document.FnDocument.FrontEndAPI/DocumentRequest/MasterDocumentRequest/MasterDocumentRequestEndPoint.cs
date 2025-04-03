using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class MasterDocumentRequestEndPoint
    {
        private const string _route = "master-document-request";
        private const string _tag = "Master Document Request";

        private readonly GetDocumentRequestDetailHandler _getDocumentRequestDetailHandler;
        private readonly GetDocumentRequestListHandler _getDocumentRequestListHandler;
        private readonly ExportExcelGetDocumentRequestListHandler _exportExcelGetDocumentRequestListHandler;
        private readonly SaveReadyDocumentHandler _saveReadyDocumentHandler;
        private readonly GetSoftCopyDocumentRequestListHandler _getSoftCopyDocumentRequestListHandler;
        private readonly SaveSoftCopyDocumentHandler _saveSoftCopyDocumentHandler;
        private readonly DeleteSoftCopyDocumentHandler _deleteSoftCopyDocumentHandler;
        private readonly CancelDocumentRequestByStaffHandler _cancelDocumentRequestByStaffHandler;
        private readonly GetVenueForCollectionHandler _getVenueForCollectionHandler;
        private readonly GetCollectorOptionListHandler _getCollectorOptionListHandler;
        private readonly SaveFinishAndCollectReqDocumentHandler _saveFinishAndCollectReqDocumentHandler;
        private readonly GetDocumentRequestDetailForEditHandler _getDocumentRequestDetailForEditHandler;
        private readonly GetDocumentRequestListWithDetailHandler _getDocumentRequestListWithDetailHandler;

        public MasterDocumentRequestEndPoint(
            GetDocumentRequestDetailHandler getDocumentRequestDetailHandler,
            GetDocumentRequestListHandler getDocumentRequestListHandler,
            ExportExcelGetDocumentRequestListHandler exportExcelGetDocumentRequestListHandler,
            SaveReadyDocumentHandler saveReadyDocumentHandler,
            GetSoftCopyDocumentRequestListHandler getSoftCopyDocumentRequestListHandler,
            SaveSoftCopyDocumentHandler saveSoftCopyDocumentHandler,
            DeleteSoftCopyDocumentHandler deleteSoftCopyDocumentHandler,
            CancelDocumentRequestByStaffHandler cancelDocumentRequestByStaffHandler,
            GetVenueForCollectionHandler getVenueForCollectionHandler,
            GetCollectorOptionListHandler getCollectorOptionListHandler,
            SaveFinishAndCollectReqDocumentHandler saveFinishAndCollectReqDocumentHandler,
            GetDocumentRequestDetailForEditHandler getDocumentRequestDetailForEditHandler,
            GetDocumentRequestListWithDetailHandler getDocumentRequestListWithDetailHandler)
        {
            _getDocumentRequestDetailHandler = getDocumentRequestDetailHandler;
            _getDocumentRequestListHandler = getDocumentRequestListHandler;
            _exportExcelGetDocumentRequestListHandler = exportExcelGetDocumentRequestListHandler;
            _saveReadyDocumentHandler = saveReadyDocumentHandler;
            _getSoftCopyDocumentRequestListHandler = getSoftCopyDocumentRequestListHandler;
            _saveSoftCopyDocumentHandler = saveSoftCopyDocumentHandler;
            _deleteSoftCopyDocumentHandler = deleteSoftCopyDocumentHandler;
            _cancelDocumentRequestByStaffHandler = cancelDocumentRequestByStaffHandler;
            _getVenueForCollectionHandler = getVenueForCollectionHandler;
            _getCollectorOptionListHandler = getCollectorOptionListHandler;
            _saveFinishAndCollectReqDocumentHandler = saveFinishAndCollectReqDocumentHandler;
            _getDocumentRequestDetailForEditHandler = getDocumentRequestDetailForEditHandler;
            _getDocumentRequestListWithDetailHandler = getDocumentRequestListWithDetailHandler;
        }

        [FunctionName(nameof(MasterDocumentRequestEndPoint.GetDocumentRequestDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Document Request Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentRequestDetailRequest.IdDocumentReqApplicant), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDocumentRequestDetailRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDocumentRequestDetailRequest.IncludePaymentInfo), In = ParameterLocation.Query, Required = true, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDocumentRequestDetailResult))]
        public Task<IActionResult> GetDocumentRequestDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-document-request-detail")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDocumentRequestDetailHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterDocumentRequestEndPoint.GetDocumentRequestList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Document Request List", 
            Description = "<b>Param Request Year:</b><br>- 1 = Within last one year<br>- 2022 = Specific year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.RequestYear), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.IdDocumentReqType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.ApprovalStatus), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.PaymentStatus), In = ParameterLocation.Query, Type = typeof(int?))]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.IdDocumentReqStatusWorkflow), In = ParameterLocation.Query, Type = typeof(int?))]
        [OpenApiParameter(nameof(GetDocumentRequestListRequest.SearchQuery), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDocumentRequestListResult))]
        public Task<IActionResult> GetDocumentRequestList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-document-request-list")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDocumentRequestListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterDocumentRequestEndPoint.ExportExcelGetDocumentRequestList))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Get Document Request List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelGetDocumentRequestListRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelGetDocumentRequestList(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-document-request-list-excel")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _exportExcelGetDocumentRequestListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterDocumentRequestEndPoint.SaveReadyDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Save Ready Document")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveReadyDocumentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveReadyDocument(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-ready-document")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveReadyDocumentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterDocumentRequestEndPoint.GetSoftCopyDocumentRequestList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Soft Copy Document Request List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSoftCopyDocumentRequestListRequest.IdDocumentReqApplicant), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSoftCopyDocumentRequestListResult>))]
        public Task<IActionResult> GetSoftCopyDocumentRequestList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-soft-copy-document-request-list")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getSoftCopyDocumentRequestListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterDocumentRequestEndPoint.SaveSoftCopyDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Save Soft Copy Document Request")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(CreateDocumentRequestStaffRequest))]
        //[OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SaveSoftCopyDocumentRequest))]
        public Task<IActionResult> SaveSoftCopyDocument(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-soft-copy-document")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _saveSoftCopyDocumentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterDocumentRequestEndPoint.DeleteSoftCopyDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Soft Copy Document Request")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteSoftCopyDocumentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteSoftCopyDocument(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-soft-copy-document")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteSoftCopyDocumentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterDocumentRequestEndPoint.CancelDocumentRequestByStaff))]
        [OpenApiOperation(tags: _tag, Summary = "Cancel Document Request By Staff")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CancelDocumentRequestByStaffRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CancelDocumentRequestByStaff(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/cancel-document-request-by-staff")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _cancelDocumentRequestByStaffHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterDocumentRequestEndPoint.GetVenueForCollection))]
        [OpenApiOperation(tags: _tag, Summary = "Get Venue for Collection")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetVenueForCollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueForCollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueForCollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetVenueForCollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetVenueForCollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueForCollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetVenueForCollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetVenueForCollectionRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetVenueForCollectionResult))]
        public Task<IActionResult> GetVenueForCollection(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-venue-for-collection")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getVenueForCollectionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterDocumentRequestEndPoint.GetCollectorOptionList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Collector Option List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCollectorOptionListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCollectorOptionListRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCollectorOptionListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCollectorOptionListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCollectorOptionListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCollectorOptionListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCollectorOptionListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetCollectorOptionListRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCollectorOptionListResult))]
        public Task<IActionResult> GetCollectorOptionList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-collector-option-list")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getCollectorOptionListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterDocumentRequestEndPoint.SaveFinishAndCollectReqDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Save Finish and Collect Document Request")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveFinishAndCollectReqDocumentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveFinishAndCollectReqDocument(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/save-finish-collect-document-request")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveFinishAndCollectReqDocumentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterDocumentRequestEndPoint.GetDocumentRequestDetailForEdit))]
        [OpenApiOperation(tags: _tag, Summary = "Get Document Request Detail For Edit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentRequestDetailForEditRequest.IdDocumentReqApplicant), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDocumentRequestDetailForEditRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDocumentRequestDetailForEditResult))]
        public Task<IActionResult> GetDocumentRequestDetailForEdit(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-document-request-detail-for-edit")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDocumentRequestDetailForEditHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterDocumentRequestEndPoint.GetDocumentRequestListWithDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Document Request List With Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentRequestListWithDetailRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDocumentRequestListWithDetailRequest.RequestYear), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetDocumentRequestListWithDetailRequest.IdDocumentReqType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestListWithDetailRequest.ApprovalStatus), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDocumentRequestListWithDetailRequest.PaymentStatus), In = ParameterLocation.Query, Type = typeof(int?))]
        [OpenApiParameter(nameof(GetDocumentRequestListWithDetailRequest.IdDocumentReqStatusWorkflow), In = ParameterLocation.Query, Type = typeof(int?))]
        [OpenApiParameter(nameof(GetDocumentRequestListWithDetailRequest.SearchQuery), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetDocumentRequestListWithDetailResult>))]
        public Task<IActionResult> GetDocumentRequestListWithDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-document-request-list-with-detail")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDocumentRequestListWithDetailHandler.Execute(req, cancellationToken);
        }
    }
}
