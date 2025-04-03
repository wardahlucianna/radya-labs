using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.User.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.User.FnUser.User
{
    public class ForgotPasswordHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly INotificationManager _notificationManager;
        protected IDbContextTransaction Transaction;

        public ForgotPasswordHandler(
            IUserDbContext dbContext,
            IConfiguration configuration,
            INotificationManager notificationManager)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _notificationManager = notificationManager;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<ForgotPasswordRequest, ForgotPasswordValidator>();

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            int maxCountResetPassword = _configuration.GetValue<int>("MaxCountResetPassword");
            var user = await _dbContext.Entity<MsUser>()
                                       .Include(x => x.UserPassword)
                                       .Include(x => x.UserSchools)
                                       .Where(x => x.Email == body.Email && x.Username == body.Username)
                                       .FirstOrDefaultAsync(CancellationToken);
            if (user is null)
                throw new NotFoundException($"User with email {body.Email} and username {body.Username} is not found");
            if (user.IsActiveDirectory)
                throw new BadRequestException("Can't reset password user from active directory");
            if (user.CountResetRequest >= maxCountResetPassword)
                throw new BadRequestException("Can't reset the password. The maximum reset count has been reached. Please contact admin to reset the maximum count.");

            var changePasswordCode = Guid.NewGuid().ToString();

            user.RequestChangePasswordCode = changePasswordCode;
            user.UsedDate = null;
            user.CountResetRequest++;
            _dbContext.Entity<MsUser>().Update(user);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);
            
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                if (user.UserSchools.Count != 0)
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(user.UserSchools.First().IdSchool, "USR9")
                    {
                        IdRecipients = new[] { user.Id },
                        KeyValues = new Dictionary<string, object>
                        {
                            { "receiverName", user.DisplayName },
                            { "email", user.Email },
                            { "passwordCode", changePasswordCode }
                        }
                    });
                    collector.Add(message);
                }
            }

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            Transaction?.Rollback();
            var response = Request.CreateApiErrorResponse(ex);

            return Task.FromResult(response as IActionResult);
        }

        protected override void OnFinally()
        {
            Transaction?.Dispose();
        }
    }

}
