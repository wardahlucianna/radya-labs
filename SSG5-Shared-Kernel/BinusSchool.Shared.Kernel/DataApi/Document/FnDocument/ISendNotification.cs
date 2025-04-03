using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.SendEmail;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface ISendNotification : IFnDocument
    {

        [Post("/document/send-email/email-blp-clearance-parent")]
        Task<ApiErrorResult> SendEmailClearancFormForParent([Body] SendEmailClearanceFormForParentRequest body);

        [Post("/document/send-email/email-blp-clearance-staff")]
        Task<ApiErrorResult> SendEmailClearancFormForStaff([Body] SendEmailClearanceFormForStaffRequest body);

        [Post("/document/send-email/email-blp-concent-parent")]
        Task<ApiErrorResult> SendEmailConcentFormForParent([Body] SendEmailConcentFormForParentRequest body);

        [Post("/document/send-email/resend-email-blp-parent")]
        Task<ApiErrorResult<ResendEmailBLPForParentResult>> ResendEmailBLPForParent([Body] List<ResendEmailBLPForParentRequest> body);

    }
}
