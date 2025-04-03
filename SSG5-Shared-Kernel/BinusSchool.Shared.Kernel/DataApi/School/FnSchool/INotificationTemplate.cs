using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.NotificationTemplate;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface INotificationTemplate : IFnSchool
    {
        [Get("/notification-template")]
        Task<ApiErrorResult<GetNotificationTemplateScenarioResult>> GetNotificationTemplateScenario(GetNotificationTemplateScenarioRequest param);
    }
}
