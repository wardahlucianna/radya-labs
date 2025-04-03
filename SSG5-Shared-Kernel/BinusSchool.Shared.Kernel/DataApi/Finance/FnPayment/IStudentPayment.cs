using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Finance.FnPayment.Invoice;
using BinusSchool.Data.Model.Finance.FnPayment.StudentPayment;
using Refit;

namespace BinusSchool.Data.Api.Finance.FnPayment
{
    public interface IStudentPayment  : IFnPayment
    {
        [Get("/invoice-student/get-student-manualpayment")]
        Task<ApiErrorResult<IEnumerable<GetStudentForManualPaymentResult>>> GetStudentForManualPayment(GetStudentForManualPaymentRequest query);

        [Get("/invoice-student/get-student-importevent-payment")]
        Task<ApiErrorResult<IEnumerable<GetStudentForImportEventResult>>> GetStudentForImportEvent(GetStudentForImportEventRequest query);

        //[Post("/invoice-student/add-invoice-student")]
        //Task<ApiErrorResult> AddInvoiceStudent([Body] AddInvoiceStudentRequest body);

        [Post("/invoice-student/add-invoice-student")]
        Task<ApiErrorResult> AddInvoiceStudent([Body] AddInvoiceStudentRequestV2 body);

        [Get("/invoice-student/get-invoice-student")]
        Task<ApiErrorResult<IEnumerable<GetInvoiceStudentResult>>> GetInvoiceStudent(GetInvoiceStudentRequest query);

        [Get("/invoice-student/get-iframe-school-fee-link")]
        Task<ApiErrorResult<GetIFrameSchoolFeeLinkResult>> GetIFrameSchoolFeeLink(GetIFrameSchoolFeeLinkRequest query);

        [Multipart]
        [Post("/invoice-student/import-excel-invoice-student")]
        Task<ApiErrorResult<ImportExcelInvoiceStudentResult>> ImportExcelInvoiceStudent(string IdEventPayment,
                                                     [AliasAs("file")] StreamPart file);

        [Post("/invoice-student/update-invoice-customer-number-send-notif")]
        Task<ApiErrorResult> UpdateCustomerNumberAndInvoiceNotif([Body] UpdateCustomerNumberAndInvoiceNotifRequest body);

        [Get("/invoice-student/get-student-payment")]
        Task<ApiErrorResult<IEnumerable<GetStudentPaymentResult>>> GetStudentPayment(GetStudentPaymentRequest query);

        [Post("/invoice-student/export-excel-daily-report-student-payment")]
        Task<HttpResponseMessage> ExportDailyReportStudentPayment([Body] GetDailyReportStudentPaymentRequest body);

        [Get("/invoice-student/get-daily-report-student-payment")]
        Task<ApiErrorResult<IEnumerable<GetDailyReportStudentPaymentResult>>> GetDailyReportStudentPayment(GetDailyReportStudentPaymentRequest query);

        [Post("/invoice-student/export-excel-student-payment")]
        Task<HttpResponseMessage> ExportExcelStudentPayment([Body] ExportExcelStudentPaymentRequest body);

        [Multipart]
        [Post("/invoice-student/add-input-manual-student-payment")]
        Task<ApiErrorResult> AddInputManualStudentPayment(string IdTransaction,
                                                string PaymentDate,
                                                string IdPaymentMethod,
                                                string Notes,
                                                string IdUserAction,
                                                [AliasAs("file")] StreamPart file);

        [Post("/invoice-student/resend-email-input-manual-student-payment")]
        Task<ApiErrorResult> ResendEmailInputManualStudentPayment([Body] ResendEmailInputManualStudentPaymentRequest body);


    }
}
