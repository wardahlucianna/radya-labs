using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IMasterDocumentRequest : IFnDocument
    {
        [Get("/master-document-request/get-document-request-detail")]
        Task<ApiErrorResult<GetDocumentRequestDetailResult>> GetDocumentRequestDetail(GetDocumentRequestDetailRequest param);

        [Get("/master-document-request/get-document-request-list")]
        Task<ApiErrorResult<IEnumerable<GetDocumentRequestListResult>>> GetDocumentRequestList(GetDocumentRequestListRequest param);

        [Post("/master-document-request/get-document-request-list-excel")]
        Task<HttpResponseMessage> ExportExcelGetDocumentRequestList([Body] ExportExcelGetDocumentRequestListRequest param);

        [Post("/master-document-request/save-ready-document")]
        Task<ApiErrorResult> SaveReadyDocument([Body] SaveReadyDocumentRequest param);

        [Get("/master-document-request/get-soft-copy-document-request-list")]
        Task<ApiErrorResult<IEnumerable<GetSoftCopyDocumentRequestListResult>>> GetSoftCopyDocumentRequestList(GetSoftCopyDocumentRequestListRequest param);

        [Multipart]
        [Post("/master-document-request/save-soft-copy-document")]
        Task<ApiErrorResult> SaveSoftCopyDocument(string IdDocumentReqApplicantDetail,
                                                    bool ShowToParent,
                                                    [AliasAs("file")] StreamPart file);

        [Delete("/master-document-request/delete-soft-copy-document")]
        Task<ApiErrorResult> DeleteSoftCopyDocument([Body] DeleteSoftCopyDocumentRequest param);

        [Delete("/master-document-request/cancel-document-request-by-staff")]
        Task<ApiErrorResult> CancelDocumentRequestByStaff([Body] CancelDocumentRequestByStaffRequest param);

        [Get("/master-document-request/get-venue-for-collection")]
        Task<ApiErrorResult<IEnumerable<GetVenueForCollectionResult>>> GetVenueForCollection(GetVenueForCollectionRequest param);

        [Get("/master-document-request/get-collector-option-list")]
        Task<ApiErrorResult<IEnumerable<GetCollectorOptionListResult>>> GetCollectorOptionList(GetCollectorOptionListRequest param);

        [Put("/master-document-request/save-finish-collect-document-request")]
        Task<ApiErrorResult> SaveFinishAndCollectReqDocument([Body] SaveFinishAndCollectReqDocumentRequest param);

        [Get("/master-document-request/get-document-request-detail-for-edit")]
        Task<ApiErrorResult<GetDocumentRequestDetailForEditResult>> GetDocumentRequestDetailForEdit(GetDocumentRequestDetailForEditRequest param);

        [Get("/master-document-request/get-document-request-list-with-detail")]
        Task<ApiErrorResult<IEnumerable<GetDocumentRequestListWithDetailResult>>> GetDocumentRequestListWithDetail(GetDocumentRequestListWithDetailRequest param);
    }
}
