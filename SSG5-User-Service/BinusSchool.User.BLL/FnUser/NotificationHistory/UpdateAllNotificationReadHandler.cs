using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.User.FnUser.NotificationHistory;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.NotificationHistory.Validator;
using Microsoft.EntityFrameworkCore;
namespace BinusSchool.User.FnUser.NotificationHistory
{
    public class UpdateAllNotificationReadHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public UpdateAllNotificationReadHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateAllNotificationReadRequest, UpdateAllNotificationReadValidator>();

            //set all notification user status
            var userNotifications = await _dbContext.Entity<TrNotificationUser>()
                                                    .Include(x => x.Notification)
                                                    .Where(x => x.IdUser == body.UserId
                                                                && x.Notification.IdSchool == body.IdSchool)
                                                    .ToListAsync(CancellationToken);
            if (userNotifications.Any())
            {
                foreach (var item in userNotifications)
                {
                    item.ReadDate = body.MarkAsRead ? DateTimeUtil.ServerTime : default(DateTime?);
                }
                _dbContext.Entity<TrNotificationUser>().UpdateRange(userNotifications);
            }

            //get all blast notification which not created on TrNotificationUser
            var blastNotHaves = await _dbContext.Entity<TrNotification>()
                                                .Include(x => x.NotificationUsers)
                                                .Where(x => x.IsBlast
                                                            && !x.NotificationUsers.Any(y => y.IdUser == body.UserId)
                                                            && x.IdSchool == body.IdSchool)
                                                .Select(x => x.Id)
                                                .ToListAsync(CancellationToken);
            if (blastNotHaves.Any())
                _dbContext.Entity<TrNotificationUser>().AddRange(blastNotHaves.Select(x => new TrNotificationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    IdNotification = x,
                    IdUser = body.UserId,
                    ReadDate = body.MarkAsRead ? DateTimeUtil.ServerTime : default(DateTime?)
                }));

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
