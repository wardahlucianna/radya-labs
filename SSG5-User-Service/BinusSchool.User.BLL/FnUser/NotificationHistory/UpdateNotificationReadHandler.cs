using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
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
    public class UpdateNotificationReadHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public UpdateNotificationReadHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateNotificationReadRequest,UpdateNotificationReadValidator>();

            // find notification per user first, blast notification not saved per user
            var userNotification = await _dbContext.Entity<TrNotificationUser>()
                                                   .Where(x => x.IdNotification == body.IdNotification
                                                               && x.IdUser == body.UserId)
                                                   .FirstOrDefaultAsync(CancellationToken);

            if (userNotification is null)
            {
                // otherwise find from notification 
                var notification = await _dbContext.Entity<TrNotification>()
                                                   .AnyAsync(x => x.Id == body.IdNotification, CancellationToken);

                if (!notification)
                    throw new NotFoundException("Notification not found");

                var newUserNotification = new TrNotificationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    IdNotification = body.IdNotification,
                    IdUser = body.UserId,
                    ReadDate = body.MarkAsRead ? DateTimeUtil.ServerTime : default(DateTime?)
                };
                _dbContext.Entity<TrNotificationUser>().Add(newUserNotification);
            }
            else
            {
                userNotification.ReadDate = body.MarkAsRead ? DateTimeUtil.ServerTime : default(DateTime?);
                _dbContext.Entity<TrNotificationUser>().Update(userNotification);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
