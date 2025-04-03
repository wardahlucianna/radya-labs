using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using Refit;

namespace BinusSchool.Data.Api.Util.FnNotification
{
    public interface ISendGrid : IFnNotification
    {
        [Get("/sendgrid/template")]
        Task<ApiErrorResult<IEnumerable<GetSendGridTemplateResult>>> GetSendGridTemplates(GetSendGridTemplateRequest request);

        [Post("/sendgrid/send-dynamic")]
        Task<ApiErrorResult<string>> AddSendGridDynamicEmail([Body] AddSendGridDynamicEmailRequest body);

        [Post("/sendgrid/send-email")]
        Task<ApiErrorResult<SendSendGridEmailResult>> SendSendGridEmail([Body] SendSendGridEmailRequest body);
    }
}
