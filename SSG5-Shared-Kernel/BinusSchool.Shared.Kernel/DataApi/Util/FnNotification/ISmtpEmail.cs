using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Util.FnNotification.SmtpEmail;
using Refit;

namespace BinusSchool.Data.Api.Util.FnNotification
{
    public interface ISmtpEmail : IFnNotification
    {
        [Post("/smpt-email/send-email")]
        Task<ApiErrorResult<SendSmtpEmailResult>> SendSmtpEmail([Body] SendSmtpEmailRequest body);
    }
}
