using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Finance.FnPayment.ExtracurricularInvoice;
using Refit;

namespace BinusSchool.Data.Api.Finance.FnPayment
{
    public interface IExtracurricularInvoice : IFnPayment
    {
        [Post("/extracurricular-invoice/get-student-extracurricular-invoice-status")]
        Task<ApiErrorResult<IEnumerable<GetStudentExtracurricularInvoiceStatusResult>>> GetStudentExtracurricularInvoiceStatus([Body] GetStudentExtracurricularInvoiceStatusRequest body);

        [Post("/extracurricular-invoice/get-student-extracurricular-invoice-status-data-table")]
        Task<ApiErrorResult<IEnumerable<GetStudentExtracurricularInvoiceStatusFormatDataTableResult>>> GetStudentExtracurricularInvoiceStatusFormatDataTable([Body] GetStudentExtracurricularInvoiceStatusRequest body);

        [Post("/extracurricular-invoice/create-student-extracurricular-invoice")]
        Task<ApiErrorResult<CreateStudentExtracurricularInvoiceResult>> CreateStudentExtracurricularInvoice([Body] List<CreateStudentExtracurricularInvoiceRequest> body);

        [Delete("/extracurricular-invoice/delete-student-extracurricular-invoice")]
        Task<ApiErrorResult<DeleteStudentExtracurricularInvoiceResult>> DeleteStudentExtracurricularInvoice([Body] DeleteStudentExtracurricularInvoiceRequest body);

        [Delete("/extracurricular-invoice/delete-all-student-extracurricular-invoice")]
        Task<ApiErrorResult<IEnumerable<DeleteAllStudentExtracurricularInvoiceResult>>> DeleteAllStudentExtracurricularInvoice([Body] DeleteAllStudentExtracurricularInvoiceRequest body);

        [Put("/extracurricular-invoice/update-due-date-extracurricular-payment")]
        Task<ApiErrorResult> UpdateDueDateExtracurricularPayment([Body] UpdateDueDateExtracurricularPaymentRequest body);
    }
}
