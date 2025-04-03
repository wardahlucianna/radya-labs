using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class DocumentRequestTypeEndPoint
    {
        private const string _route = "document-request-type";
        private const string _tag = "Master Document Request Type";

        private readonly GetDocumentRequestTypeListHandler _getDocumentRequestTypeListHandler;
        private readonly UpdateDocumentTypeStatusHandler _updateDocumentTypeStatusHandler;
        private readonly ExportExcelDocumentTypeListHandler _exportExcelDocumentTypeListHandler;
        private readonly GetDocumentRequestTypeDetailHandler _getDocumentRequestTypeDetailHandler;
        private readonly GetDocumentRequestFieldTypeHandler _getDocumentRequestFieldTypeHandler;
        private readonly GetOptionCategoryByFieldTypeHandler _getOptionCategoryByFieldTypeHandler;
        private readonly GetOptionsByOptionCategoryHandler _getOptionsByOptionCategoryHandler;
        private readonly AddDocumentRequestTypeHandler _addDocumentRequestTypeHandler;
        private readonly AddDocumentRequestOptionCategoryHandler _addDocumentRequestOptionCategoryHandler;
        private readonly UpdateDocumentRequestOptionCategoryHandler _updateDocumentRequestOptionCategoryHandler;
        private readonly UpdateDocumentRequestTypeHandler _updateDocumentRequestTypeHandler;
        private readonly DeleteDocumentRequestTypeHandler _deleteDocumentRequestTypeHandler;
        private readonly UpdateImportedDataOptionCategoryHandler _updateImportedDataOptionCategoryHandler;

        public DocumentRequestTypeEndPoint(
            GetDocumentRequestTypeListHandler getDocumentRequestTypeListHandler,
            UpdateDocumentTypeStatusHandler updateDocumentTypeStatusHandler,
            ExportExcelDocumentTypeListHandler exportExcelDocumentTypeListHandler,
            GetDocumentRequestTypeDetailHandler getDocumentRequestTypeDetailHandler,
            GetDocumentRequestFieldTypeHandler getDocumentRequestFieldTypeHandler,
            GetOptionCategoryByFieldTypeHandler getOptionCategoryByFieldTypeHandler,
            GetOptionsByOptionCategoryHandler getOptionsByOptionCategoryHandler,
            AddDocumentRequestTypeHandler addDocumentRequestTypeHandler,
            AddDocumentRequestOptionCategoryHandler addDocumentRequestOptionCategoryHandler,
            UpdateDocumentRequestOptionCategoryHandler updateDocumentRequestOptionCategoryHandler,
            UpdateDocumentRequestTypeHandler updateDocumentRequestTypeHandler,
            DeleteDocumentRequestTypeHandler deleteDocumentRequestTypeHandler,
            UpdateImportedDataOptionCategoryHandler updateImportedDataOptionCategoryHandler)
        {
            _getDocumentRequestTypeListHandler = getDocumentRequestTypeListHandler;
            _updateDocumentTypeStatusHandler = updateDocumentTypeStatusHandler;
            _exportExcelDocumentTypeListHandler = exportExcelDocumentTypeListHandler;
            _getDocumentRequestTypeDetailHandler = getDocumentRequestTypeDetailHandler;
            _getDocumentRequestFieldTypeHandler = getDocumentRequestFieldTypeHandler;
            _getOptionCategoryByFieldTypeHandler = getOptionCategoryByFieldTypeHandler;
            _getOptionsByOptionCategoryHandler = getOptionsByOptionCategoryHandler;
            _addDocumentRequestTypeHandler = addDocumentRequestTypeHandler;
            _addDocumentRequestOptionCategoryHandler = addDocumentRequestOptionCategoryHandler;
            _updateDocumentRequestOptionCategoryHandler = updateDocumentRequestOptionCategoryHandler;
            _updateDocumentRequestTypeHandler = updateDocumentRequestTypeHandler;
            _deleteDocumentRequestTypeHandler = deleteDocumentRequestTypeHandler;
            _updateImportedDataOptionCategoryHandler = updateImportedDataOptionCategoryHandler;
        }

        [FunctionName(nameof(DocumentRequestTypeEndPoint.GetDocumentRequestTypeList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Document Request Type List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.AcademicYearCode), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.LevelCode), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.GradeCode), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.ActiveStatus), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.VisibleToParent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDocumentRequestTypeListRequest.PaidDocument), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetDocumentRequestTypeListResult>))]
        public Task<IActionResult> GetDocumentRequestTypeList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-document-request-type-list")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDocumentRequestTypeListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestTypeEndPoint.UpdateDocumentTypeStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Update Document Request Type Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateDocumentTypeStatusRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateDocumentTypeStatus(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-document-type-status")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateDocumentTypeStatusHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestTypeEndPoint.ExportExcelDocumentTypeList))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Document Type List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelDocumentTypeListRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelDocumentTypeList(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-document-request-type-list-excel")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _exportExcelDocumentTypeListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestTypeEndPoint.GetDocumentRequestTypeDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Document Request Type Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentRequestTypeDetailRequest.IdDocumentReqType), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDocumentRequestTypeDetailResult))]
        public Task<IActionResult> GetDocumentRequestTypeDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-document-request-type-detail")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDocumentRequestTypeDetailHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestTypeEndPoint.GetDocumentRequestFieldType))]
        [OpenApiOperation(tags: _tag, Summary = "Get Document Request Field Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentRequestFieldTypeRequest.IdDocumentReqFieldType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetDocumentRequestFieldTypeResult>))]
        public Task<IActionResult> GetDocumentRequestFieldType(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-document-request-field-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDocumentRequestFieldTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestTypeEndPoint.GetOptionCategoryByFieldType))]
        [OpenApiOperation(tags: _tag, Summary = "Get Option Category by Field Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetOptionCategoryByFieldTypeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetOptionCategoryByFieldTypeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetOptionCategoryByFieldTypeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetOptionCategoryByFieldTypeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetOptionCategoryByFieldTypeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetOptionCategoryByFieldTypeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetOptionCategoryByFieldTypeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetOptionCategoryByFieldTypeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetOptionCategoryByFieldTypeRequest.IdDocumentReqFieldType), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetOptionCategoryByFieldTypeResult>))]
        public Task<IActionResult> GetOptionCategoryByFieldType(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-option-category-by-field-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getOptionCategoryByFieldTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestTypeEndPoint.GetOptionsByOptionCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Options by Option Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetOptionsByOptionCategoryRequest.IdDocumentReqOptionCategory), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetOptionsByOptionCategoryRequest.IdDocumentReqOptionChosenList), Type = typeof(string[]), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetOptionsByOptionCategoryResult))]
        public Task<IActionResult> GetOptionsByOptionCategory(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-options-by-option-category")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getOptionsByOptionCategoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestTypeEndPoint.AddDocumentRequestType))]
        [OpenApiOperation(tags: _tag, Summary = "Add Document Request Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddDocumentRequestTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddDocumentRequestType(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-document-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _addDocumentRequestTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestTypeEndPoint.AddDocumentRequestOptionCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Add Document Request Option Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddDocumentRequestOptionCategoryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddDocumentRequestOptionCategory(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-document-option-category")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _addDocumentRequestOptionCategoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestTypeEndPoint.UpdateDocumentRequestOptionCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Update Document Request Option Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateDocumentRequestOptionCategoryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateDocumentRequestOptionCategory(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-document-option-category")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateDocumentRequestOptionCategoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestTypeEndPoint.UpdateDocumentRequestType))]
        [OpenApiOperation(tags: _tag, Summary = "Update Document Request Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateDocumentRequestTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateDocumentRequestType(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-document-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateDocumentRequestTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestTypeEndPoint.DeleteDocumentRequestType))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Document Request Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteDocumentRequestTypeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteDocumentRequestType(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-document-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteDocumentRequestTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestTypeEndPoint.UpdateImportedDataOptionCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Update Imported Data Option Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateImportedDataOptionCategoryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateImportedDataOptionCategory(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-imported-data-option-category")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateImportedDataOptionCategoryHandler.Execute(req, cancellationToken);
        }
    }
}
