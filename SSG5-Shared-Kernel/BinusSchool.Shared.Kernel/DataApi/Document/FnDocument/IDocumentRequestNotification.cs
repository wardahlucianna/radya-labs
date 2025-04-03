using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestNotification;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IDocumentRequestNotification : IFnDocument
    {
        [Post("/document-request-notification/send-email-new-request-to-parent")]
        Task<ApiErrorResult<SendEmailNewRequestToParentResult>> SendEmailNewRequestToParent([Body] SendEmailNewRequestToParentRequest param);

        [Post("/document-request-notification/send-email-cancel-request-to-parent")]
        Task<ApiErrorResult<SendEmailCancelRequestToParentResult>> SendEmailCancelRequestToParent([Body] SendEmailCancelRequestToParentRequest param);

        [Post("/document-request-notification/send-email-cancel-request-to-staff")]
        Task<ApiErrorResult<SendEmailCancelRequestToStaffResult>> SendEmailCancelRequestToStaff([Body] SendEmailCancelRequestToStaffRequest param);

        [Post("/document-request-notification/send-email-need-process-request-to-pic")]
        Task<ApiErrorResult<SendEmailNeedProcessRequestToPICResult>> SendEmailNeedProcessRequestToPIC([Body] SendEmailNeedProcessRequestToPICRequest param);

        [Post("/document-request-notification/send-email-update-request-status-to-parent")]
        Task<ApiErrorResult<SendEmailUpdateRequestStatusToParentResult>> SendEmailUpdateRequestStatusToParent([Body] SendEmailUpdateRequestStatusToParentRequest param);


        [Post("/document-request-notification/send-email-finished-request-to-parent")]
        Task<ApiErrorResult<SendEmailFinishedRequestToParentResult>> SendEmailFinishedRequestToParent([Body] SendEmailFinishedRequestToParentRequest param);

        [Post("/document-request-notification/send-email-collected-request-to-parent")]
        Task<ApiErrorResult<SendEmailCollectedRequestToParentResult>> SendEmailCollectedRequestToParent([Body] SendEmailCollectedRequestToParentRequest param);

        [Post("/document-request-notification/send-email-data-update-request-to-parent")]
        Task<ApiErrorResult<SendEmailDataUpdateRequestToParentResult>> SendEmailDataUpdateRequestToParent([Body] SendEmailDataUpdateRequestToParentRequest param);

        [Post("/document-request-notification/send-email-data-update-request-to-parent")]
        Task<ApiErrorResult<SendEmailDataUpdateRequestToStaffResult>> SendEmailDataUpdateRequestToStaff([Body] SendEmailDataUpdateRequestToStaffRequest param);

        [Post("/document-request-notification/send-email-need-approval-request-to-approver")]
        Task<ApiErrorResult<SendEmailNeedApprovalRequestToApproverResult>> SendEmailNeedApprovalRequestToApprover([Body] SendEmailNeedApprovalRequestToApproverRequest param);

        [Post("/document-request-notification/send-email-need-payment-verificatrion-request-to-approver")]
        Task<ApiErrorResult<SendEmailNeedPaymentVerificationRequestToApproverResult>> SendEmailNeedPaymentVerificationRequestToApprover([Body] SendEmailNeedPaymentVerificationRequestToApproverRequest param);
    }
}
