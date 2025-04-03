using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.NotificationHistory;
using Refit;

namespace BinusSchool.Data.Api.User.FnUser
{
    public interface INotificationHistory : IFnUser
    {
        [Get("/notification-history")]
        Task<ApiErrorResult<IEnumerable<GetNotificationHistoryResult>>> GetNotificationHistories(GetNotificationHistoryRequest request);

        [Get("/notification-history/badge")]
        Task<ApiErrorResult<int?>> GetNotificationBadge(GetNotificationBadgeRequest request);

        [Put("/notification-history/read")]
        Task<ApiErrorResult> UpdateNotificationReadStatus([Body] UpdateNotificationReadRequest body);

        [Put("/notification-history/read-all")]
        Task<ApiErrorResult> UpdateAllNotificationReadStatus([Body] UpdateAllNotificationReadRequest body);
    }
}
