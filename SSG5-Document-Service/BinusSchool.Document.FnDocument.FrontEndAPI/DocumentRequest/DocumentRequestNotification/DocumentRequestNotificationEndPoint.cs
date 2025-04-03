using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestNotification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestNotification
{
    public class DocumentRequestNotificationEndPoint
    {
        private const string _route = "document-request-notification";
        private const string _tag = "Document Request Notification";

        private readonly SendEmailNewRequestToParentHandler _sendEmailNewRequestToParentHandler;
        private readonly SendEmailCancelRequestToParentHandler _sendEmailCancelRequestToParentHandler;
        private readonly SendEmailCancelRequestToStaffHandler _sendEmailCancelRequestToStaffHandler;
        private readonly SendEmailNeedProcessRequestToPICHandler _sendEmailNeedProcessRequestToPICHandler;
        private readonly SendEmailUpdateRequestStatusToParentHandler _sendEmailUpdateRequestStatusToParentHandler;
        private readonly SendEmailFinishedRequestToParentHandler _sendEmailFinishedRequestToParentHandler;
        private readonly SendEmailCollectedRequestToParentHandler _sendEmailCollectedRequestToParentHandler;
        private readonly SendEmailDataUpdateRequestToParentHandler _sendEmailDataUpdateRequestToParentHandler;
        private readonly SendEmailDataUpdateRequestToStaffHandler _sendEmailDataUpdateRequestToStaffHandler;
        private readonly SendEmailNeedApprovalRequestToApproverHandler _sendEmailNeedApprovalRequestToApproverHandler;
        private readonly SendEmailNeedPaymentVerificationRequestToApproverHandler _sendEmailNeedPaymentVerificationRequestToApproverHandler;

        public DocumentRequestNotificationEndPoint(
            SendEmailNewRequestToParentHandler sendEmailNewRequestToParentHandler,
            SendEmailCancelRequestToParentHandler sendEmailCancelRequestToParentHandler,
            SendEmailCancelRequestToStaffHandler sendEmailCancelRequestToStaffHandler,
            SendEmailNeedProcessRequestToPICHandler sendEmailNeedProcessRequestToPICHandler,
            SendEmailUpdateRequestStatusToParentHandler sendEmailUpdateRequestStatusToParentHandler,
            SendEmailFinishedRequestToParentHandler sendEmailFinishedRequestToParentHandler,
            SendEmailCollectedRequestToParentHandler sendEmailCollectedRequestToParentHandler,
            SendEmailDataUpdateRequestToParentHandler sendEmailDataUpdateRequestToParentHandler,
            SendEmailDataUpdateRequestToStaffHandler sendEmailDataUpdateRequestToStaffHandler,
            SendEmailNeedApprovalRequestToApproverHandler sendEmailNeedApprovalRequestToApproverHandler,
            SendEmailNeedPaymentVerificationRequestToApproverHandler sendEmailNeedPaymentVerificationRequestToApproverHandler)
        {
            _sendEmailNewRequestToParentHandler = sendEmailNewRequestToParentHandler;
            _sendEmailCancelRequestToParentHandler = sendEmailCancelRequestToParentHandler;
            _sendEmailCancelRequestToStaffHandler = sendEmailCancelRequestToStaffHandler;
            _sendEmailNeedProcessRequestToPICHandler = sendEmailNeedProcessRequestToPICHandler;
            _sendEmailUpdateRequestStatusToParentHandler = sendEmailUpdateRequestStatusToParentHandler;
            _sendEmailFinishedRequestToParentHandler = sendEmailFinishedRequestToParentHandler;
            _sendEmailCollectedRequestToParentHandler = sendEmailCollectedRequestToParentHandler;
            _sendEmailDataUpdateRequestToParentHandler = sendEmailDataUpdateRequestToParentHandler;
            _sendEmailDataUpdateRequestToStaffHandler = sendEmailDataUpdateRequestToStaffHandler;
            _sendEmailNeedApprovalRequestToApproverHandler = sendEmailNeedApprovalRequestToApproverHandler;
            _sendEmailNeedPaymentVerificationRequestToApproverHandler = sendEmailNeedPaymentVerificationRequestToApproverHandler;
        }

        [FunctionName(nameof(DocumentRequestNotificationEndPoint.SendEmailNewRequestToParent))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email New Request to Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailNewRequestToParentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SendEmailNewRequestToParentResult))]
        public Task<IActionResult> SendEmailNewRequestToParent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email-new-request-to-parent")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _sendEmailNewRequestToParentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestNotificationEndPoint.SendEmailCancelRequestToParent))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Cancel Request to Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailCancelRequestToParentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SendEmailCancelRequestToParentResult))]
        public Task<IActionResult> SendEmailCancelRequestToParent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email-cancel-request-to-parent")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _sendEmailCancelRequestToParentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestNotificationEndPoint.SendEmailCancelRequestToStaff))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Cancel Request to Staff")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailCancelRequestToParentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SendEmailCancelRequestToParentResult))]
        public Task<IActionResult> SendEmailCancelRequestToStaff(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email-cancel-request-to-staff")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _sendEmailCancelRequestToStaffHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestNotificationEndPoint.SendEmailNeedProcessRequestToPIC))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Need Process Request to PIC")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailCancelRequestToParentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SendEmailCancelRequestToParentResult))]
        public Task<IActionResult> SendEmailNeedProcessRequestToPIC(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email-need-process-request-to-pic")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _sendEmailNeedProcessRequestToPICHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestNotificationEndPoint.SendEmailUpdateRequestStatusToParent))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Update Document Request Status to Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailUpdateRequestStatusToParentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SendEmailUpdateRequestStatusToParentResult))]
        public Task<IActionResult> SendEmailUpdateRequestStatusToParent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email-update-request-status-to-parent")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _sendEmailUpdateRequestStatusToParentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestNotificationEndPoint.SendEmailFinishedRequestToParent))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Finished Request to Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailFinishedRequestToParentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SendEmailFinishedRequestToParentResult))]
        public Task<IActionResult> SendEmailFinishedRequestToParent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email-finished-request-to-parent")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _sendEmailFinishedRequestToParentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestNotificationEndPoint.SendEmailCollectedRequestToParent))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Collected Request to Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailCollectedRequestToParentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SendEmailCollectedRequestToParentResult))]
        public Task<IActionResult> SendEmailCollectedRequestToParent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email-collected-request-to-parent")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _sendEmailCollectedRequestToParentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestNotificationEndPoint.SendEmailDataUpdateRequestToParent))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Data Update Request to Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailDataUpdateRequestToParentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SendEmailDataUpdateRequestToParentResult))]
        public Task<IActionResult> SendEmailDataUpdateRequestToParent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email-data-update-request-to-parent")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _sendEmailDataUpdateRequestToParentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestNotificationEndPoint.SendEmailDataUpdateRequestToStaff))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Data Update Request to Staff")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailDataUpdateRequestToParentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SendEmailDataUpdateRequestToParentResult))]
        public Task<IActionResult> SendEmailDataUpdateRequestToStaff(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email-data-update-request-to-staff")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _sendEmailDataUpdateRequestToStaffHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestNotificationEndPoint.SendEmailNeedApprovalRequestToApprover))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Need Approval Request to Approver")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailNeedApprovalRequestToApproverRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SendEmailNeedApprovalRequestToApproverResult))]
        public Task<IActionResult> SendEmailNeedApprovalRequestToApprover(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email-need-approval-request-to-approver")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _sendEmailNeedApprovalRequestToApproverHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DocumentRequestNotificationEndPoint.SendEmailNeedPaymentVerificationRequestToApprover))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Need Payment Verification Request to Approver")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SendEmailNeedPaymentVerificationRequestToApproverRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SendEmailNeedPaymentVerificationRequestToApproverResult))]
        public Task<IActionResult> SendEmailNeedPaymentVerificationRequestToApprover(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/send-email-need-payment-verificatrion-request-to-approver")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _sendEmailNeedPaymentVerificationRequestToApproverHandler.Execute(req, cancellationToken);
        }
    }
}
