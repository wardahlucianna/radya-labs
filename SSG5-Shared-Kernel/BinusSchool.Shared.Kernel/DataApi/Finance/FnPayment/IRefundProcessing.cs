using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing;
using Refit;

namespace BinusSchool.Data.Api.Finance.FnPayment
{
    public interface IRefundProcessing : IFnPayment
    {
        [Get("/refund-processing/get-list-student-forRefund")]
        Task<ApiErrorResult<List<GetListStudentForRefundResult>>> GetListStudentForRefund(GetListStudentForRefundRequest query);

        [Get("/refund-processing/get-list-refund-processing")]
        Task<ApiErrorResult<List<GetListRefundProcessingResult>>> GetListRefundProcessing(GetListRefundProcessingRequest query);

        [Get("/refund-processing/get-list-detail-student-refund-processing")]
        Task<ApiErrorResult<List<GetListDetailStudentRefundProcessingResult>>> GetListDetailStudentRefundProcessing(GetListDetailStudentRefundProcessingRequest query);

        [Get("/refund-processing/get-detail-refund-processing")]
        Task<ApiErrorResult<GetDetailRefundProcessingResult>> GetDetailRefundProcessing(GetDetailRefundProcessingRequest query);

        [Post("/refund-processing/add-refund-processing")]
        Task<ApiErrorResult<string>> AddRefundProcessing([Body] AddRefundProcessingRequest body);

        [Post("/refund-processing/add-student-forRefund")]
        Task<ApiErrorResult> AddStudentForRefund([Body] List<AddRefundStudentRequest> body);

        [Post("/refund-processing/update-eprf-status")]
        Task<ApiErrorResult> UpdateEPRFStatus([Body] UpdateEPRFStatusRequest body);

        [Post("/refund-processing/update-sendNotification-refund-payment")]
        Task<ApiErrorResult> UpdateIsSendNotificationRefundPayment([Body] UpdateIsSendNotificationRefundPaymentRequest body);

        [Delete("/refund-processing/delete-refund-processing")]
        Task<ApiErrorResult> DeleteRefundProcessing([Body] DeleteRefundProcessingRequest body);
        
        [Delete("/refund-processing/delete-student-forRefund")]
        Task<ApiErrorResult> DeleteStudentForRefund([Body] List<DeleteStudentForRefundRequest> body);

        [Post("/refund-processing/export-excel-refund-processing")]
        Task<HttpResponseMessage> ExportExcelRefundProcessing([Body] ExportExcelRefundProcessingRequest body);

        [Post("/refund-processing/export-excel-detail-refund-processing")]
        Task<HttpResponseMessage> ExportExcelDetailRefundProcessing([Body] ExportExcelDetailRefundProcessingRequest body);

        [Post("/refund-processing/create-student-refund-payment")]
        Task<ApiErrorResult<CreateStudentRefundPaymentResult>> CreateStudentRefundPayment([Body] CreateStudentRefundPaymentRequest body);

        [Get("/refund-processing/get-list-student-by-event-for-refund")]
        Task<ApiErrorResult<IEnumerable<GetListStudentByEventForRefundResult>>> GetListStudentByEventForRefund(GetListStudentByEventForRefundRequest param);

        [Get("/refund-processing/get-cost-center")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetCostCenter(GetCostCenterRequest param);

        [Get("/refund-processing/get-project")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetProject(GetProjectRequest param);

        [Post("/refund-processing/create-supplier-invoice-log")]
        Task<ApiErrorResult> CreateSupplierInvoiceLog([Body] CreateSupplierInvoiceLogRequest body);
    }
}
