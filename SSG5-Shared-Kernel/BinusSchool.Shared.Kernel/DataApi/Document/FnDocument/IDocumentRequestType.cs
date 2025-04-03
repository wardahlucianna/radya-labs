using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IDocumentRequestType : IFnDocument
    {
        [Get("/document-request-type/get-document-request-type-list")]
        Task<ApiErrorResult<IEnumerable<GetDocumentRequestTypeListResult>>> GetDocumentRequestTypeList(GetDocumentRequestTypeListRequest param);

        [Put("/document-request-type/update-document-type-status")]
        Task<ApiErrorResult> UpdateDocumentTypeStatus([Body] UpdateDocumentTypeStatusRequest param);

        [Post("/document-request-type/get-document-request-type-list-excel")]
        Task<HttpResponseMessage> ExportExcelDocumentTypeList([Body] ExportExcelDocumentTypeListRequest param);

        [Get("/document-request-type/get-document-request-type-detail")]
        Task<ApiErrorResult<GetDocumentRequestTypeDetailResult>> GetDocumentRequestTypeDetail(GetDocumentRequestTypeDetailRequest param);

        [Get("/document-request-type/get-document-request-field-type")]
        Task<ApiErrorResult<IEnumerable<GetDocumentRequestFieldTypeResult>>> GetDocumentRequestFieldType(GetDocumentRequestFieldTypeRequest param);

        [Get("/document-request-type/get-option-category-by-field-type")]
        Task<ApiErrorResult<IEnumerable<GetOptionCategoryByFieldTypeResult>>> GetOptionCategoryByFieldType(GetOptionCategoryByFieldTypeRequest param);

        [Get("/document-request-type/get-options-by-option-category")]
        Task<ApiErrorResult<GetOptionsByOptionCategoryResult>> GetOptionsByOptionCategory(GetOptionsByOptionCategoryRequest param);

        [Post("/document-request-type/add-document-type")]
        Task<ApiErrorResult> AddDocumentRequestType([Body] AddDocumentRequestTypeRequest param);

        [Post("/document-request-type/add-document-option-category")]
        Task<ApiErrorResult> AddDocumentRequestOptionCategory([Body] AddDocumentRequestOptionCategoryRequest param);

        [Put("/document-request-type/update-document-option-category")]
        Task<ApiErrorResult> UpdateDocumentRequestOptionCategory([Body] UpdateDocumentRequestOptionCategoryRequest param);

        [Put("/document-request-type/update-document-type")]
        Task<ApiErrorResult> UpdateDocumentRequestType([Body] UpdateDocumentRequestTypeRequest param);

        [Delete("/document-request-type/delete-document-type")]
        Task<ApiErrorResult> DeleteDocumentRequestType([Body] DeleteDocumentRequestTypeRequest param);

        [Put("/document-request-type/update-imported-data-option-category")]
        Task<ApiErrorResult> UpdateImportedDataOptionCategory([Body] UpdateImportedDataOptionCategoryRequest param);
    }
}
