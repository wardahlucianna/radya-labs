using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Finance.FnPayment.InvoiceApproval;
using Refit;

namespace BinusSchool.Data.Api.Finance.FnPayment
{
    public interface IInvoiceApproval : IFnPayment
    {
        [Get("/invoice-approval/get-list-invoice-approval")]
        Task<ApiErrorResult<List<GetListInvoiceApprovalResult>>> GetListInvoiceApproval(GetListInvoiceApprovalRequest query);

        [Get("/invoice-approval/get-list-student-invoice")]
        Task<ApiErrorResult<GetListStudentInvoiceResult>> GetListStudentInvoice(GetListStudentInvoiceRequest query);

        [Get("/invoice-approval/get-student-invoice-approval-details")]
        Task<ApiErrorResult<List<GetStudentInvoiceApprovalDetailResult>>> GetStudentInvoiceApprovalDetails(GetStudentInvoiceApprovalDetailRequest query);

        [Post("/invoice-approval/set-approval-invoice")]
        Task<ApiErrorResult> SetApprovalInvoice([Body] SetApprovalInvoiceRequest body);
    }
}
