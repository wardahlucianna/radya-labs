using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.NotificationHistory;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.NotificationHistory
{
    public class GetNotificationBadgeHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetNotificationBadgeHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetNotificationBadgeRequest>(nameof(GetNotificationBadgeRequest.IdSchool),
                                                                            nameof(GetNotificationBadgeRequest.UserId));

            var unread = await _dbContext.Entity<TrNotificationUser>()
                                         .Include(x => x.Notification)
                                         .Where(x => x.Notification.NotificationType == Common.Model.Enums.NotificationType.General
                                                     && x.IdUser == param.UserId
                                                     && x.Notification.IdSchool == param.IdSchool
                                                     && !x.ReadDate.HasValue)
                                         .CountAsync(CancellationToken);

            var unreadBlast = await _dbContext.Entity<TrNotification>()
                                              .Include(x => x.NotificationUsers)
                                              .Where(x => x.IsBlast
                                                          && x.NotificationType == Common.Model.Enums.NotificationType.General
                                                          && x.IdSchool == param.IdSchool
                                                          && !x.NotificationUsers.Any(y => y.IdUser == param.UserId))
                                              .CountAsync(CancellationToken);

            return Request.CreateApiResult2(unread + unreadBlast as object);
        }
    }
}
