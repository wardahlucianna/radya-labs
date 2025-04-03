using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IDocumentRequestPayment : IFnDocument
    {
        [Get("/document-request-payment/get-detail-document-request-payment-confirmation")]
        Task<ApiErrorResult<GetDetailDocumentRequestPaymentConfirmationResult>> GetDetailDocumentRequestPaymentConfirmation(GetDetailDocumentRequestPaymentConfirmationRequest param);

        [Multipart]
        [Post("/document-request-payment/add-payment-confirmation")]
        Task<ApiErrorResult> AddPaymentConfirmation(string IdDocumentReqApplicant,
                                                    decimal PaidAmount,
                                                    string PaymentDate,
                                                    string IdDocumentReqPaymentMethod,
                                                    string SenderAccountName,
                                                    [AliasAs("file")] StreamPart file);

        [Get("/document-request-payment/get-payment-method-by-school")]
        Task<ApiErrorResult<IEnumerable<GetPaymentMethodBySchoolResult>>> GetPaymentMethodBySchool(GetPaymentMethodBySchoolRequest param);

        [Multipart]
        [Post("/document-request-payment/upload-transfer-evidance-document")]
        Task<ApiErrorResult<UploadTransferEvidanceDocumentResult>> UploadTransferEvidanceDocument(string IdStudent,
                                                    [AliasAs("file")] StreamPart file);

        [Get("/document-request-payment/get-document-request-payment-info")]
        Task<ApiErrorResult<IEnumerable<GetDocumentRequestPaymentInfoResult>>> GetDocumentRequestPaymentInfo(GetDocumentRequestPaymentInfoRequest param);

        [Multipart]
        [Post("/document-request-payment/save-payment-approval")]
        Task<ApiErrorResult> SavePaymentApproval(string IdDocumentReqApplicant,
                                                    string PaymentDate,
                                                    string IdDocumentReqPaymentMethod,
                                                    decimal PaidAmount,
                                                    string SenderAccountName,
                                                    bool VerificationStatus,
                                                    string Remarks,
                                                    [AliasAs("file")] StreamPart file);

        [Post("/document-request-payment/get-document-request-payment-excel")]
        Task<HttpResponseMessage> ExportExcelDocumentRequestPayment([Body] ExportExcelDocumentRequestPaymentRequest param);

        [Get("/document-request-payment/get-payment-recap-list")]
        Task<ApiErrorResult<IEnumerable<GetPaymentRecapListResult>>> GetPaymentRecapList(GetPaymentRecapListRequest param);

        [Post("/document-request-payment/get-payment-recap-list-excel")]
        Task<HttpResponseMessage> ExportExcelPaymentRecapList([Body] ExportExcelPaymentRecapListRequest param);
    }
}
