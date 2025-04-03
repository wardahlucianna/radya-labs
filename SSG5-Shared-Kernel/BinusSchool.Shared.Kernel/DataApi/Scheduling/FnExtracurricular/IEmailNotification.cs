using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.SendEmail;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IEmailNotification : IFnExtracurricular
    {
        [Post("/extracurricular/send-email/email-cancel-parent")]
        Task<ApiErrorResult> SendEmailCancelledExtracurricularToParent([Body] List<SendEmailCancelExtracurricularToParentRequest> query);

        [Post("/extracurricular/send-email/email-delete-not-paid-parent")]
        Task<ApiErrorResult> SendEmailDeleteNotPaidExtracurricularToParent([Body] List<SendEmailDeleteNotPaidExtracurricularToParentRequest> query);
    }
}
