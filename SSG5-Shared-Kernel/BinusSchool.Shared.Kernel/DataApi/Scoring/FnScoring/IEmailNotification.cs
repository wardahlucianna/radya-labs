using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.SendEmail;
using BinusSchool.Data.Model.Scoring.FnScoring.SendEmail.ApprovalByEmail;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IEmailNotification : IFnScoring
    {
        [Post("/scoring/send-email/SendApprovalScoreToStaff")]
        Task<ApiErrorResult> SendEmailApprovalForScore(SendEmailApprovalForScoreRequest query);

        [Post("/scoring/send-email/SendApprovedScoreToStaff")]
        Task<ApiErrorResult> SendEmailApprovedForScore(SendEmailApprovedForScoreRequest query);

        [Post("/scoring/send-email/SendNotificationForProgressStatus")]
        Task<ApiErrorResult> SendEmailNotificationForProgressStatus(SendEmailNotificationForProgressStatusRequest query);

        [Post("/scoring/send-email/SendApprovalForProgressStatus")]
        Task<ApiErrorResult> SendEmailApprovalForProgressStatus(SendEmailApprovalForProgressStatusRequest query);

        [Post("/scoring/send-email/CreateApprovalToken")]
        Task<ApiErrorResult<CreateApprovalTokenResult>> CreateApprovalToken(CreateApprovalTokenRequest query);

        [Post("/scoring/send-email/ValidateApprovalToken")]
        Task<ApiErrorResult> ValidateApprovalToken(ValidateApprovalTokenRequest query);

        [Post("/scoring/send-email/SendEmailCompleteGenerateReport")]
        Task<bool> SendEmailCompleteGenerateReport(SendEmailCompleteGenerateReportRequest query);

        [Post("/scoring/send-email/SendEmailNotificationUnsubmittedScore")]
        Task<ApiErrorResult> SendEmailNotificationUnsubmittedScore(SendEmailNotificationUnsubmittedScoreRequest query);
    }
}
