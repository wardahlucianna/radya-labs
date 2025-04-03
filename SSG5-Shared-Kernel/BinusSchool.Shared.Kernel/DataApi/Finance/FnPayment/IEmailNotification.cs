using System.Threading.Tasks;
using Refit;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Finance.FnPayment.SendEmail;
using System;

namespace BinusSchool.Data.Api.Finance.FnPayment
{
    public interface IEmailNotification : IFnPayment
    {
        [Post("/payment/send-email/email-approver-invoice")]
        Task<ApiErrorResult> SendEmailApproverInvoice(SendEmailApproverInvoiceRequest query);

        [Post("/payment/send-email/email-refund-parent")]
        Task<ApiErrorResult> SendEmailRefundPaymentToParent(SendEmailRefundPaymentToParentRequest query);

        [Post("/payment/send-email/bulk-email-invoice-parent")]
        Task<ApiErrorResult> SendEmailBulkInvoiceToParent([Body] SendEmailBulkInvoiceToParentRequest query);

        [Post("/payment/send-email/add-queue-send-email-invoice-parent")]
        Task<ApiErrorResult> AddQueueSendEmailInvoiceToParent([Body] AddQueueSendEmailInvoiceToParentRequest query);

        [Post("/payment/send-email/bulk-email-paid-invoice-parent")]
        Task<ApiErrorResult> SendEmailBulkPaidNotificationToParent([Body] SendEmailBulkPaidNotificationToParentRequest query);

        [Post("/payment/send-email/email-notification-refund-payment-to-staff")]
        Task<ApiErrorResult> SendEmailNotificationRefundPaymentToStaff(SendEmailRefundPaymentToStaffRequest query);
    }
}
