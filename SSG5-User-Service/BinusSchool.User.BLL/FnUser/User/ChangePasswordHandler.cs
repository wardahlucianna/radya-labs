using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.User.Validator;
using BinusSchool.User.FnUser.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.User.FnUser.User
{
    public class ChangePasswordHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        protected IDbContextTransaction Transaction;
        private readonly IMachineDateTime _dateTime;

        public ChangePasswordHandler(
            IUserDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<ChangePasswordRequest, ChangePasswordValidator>();
            if (!body.NewPassword.ValidatePassword())
                throw new BadRequestException("Password must be at least 8 character with combination of uppercase and lowercase characters and number");

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var user = await _dbContext.Entity<MsUser>()
                                       .Include(x => x.UserPassword)
                                       .Where(x => x.RequestChangePasswordCode == body.RequestCode)
                                       .SingleOrDefaultAsync();
            if (user is null)
                throw new BadRequestException("this link is expired, please check your email to get the newest link to reset your password");
            if (user.UsedDate.HasValue)
                throw new BadRequestException("This link is already used, If you forgot your password, please make new request to reset password");

            if (user.UserPassword != null)
            {
                if (user.UserPassword.HashedPassword == (body.NewPassword + user.UserPassword.Salt).ToSHA512())
                    throw new BadRequestException("Your new password must be different with your previous password");

            }

            user.UsedDate = _dateTime.ServerTime;
            _dbContext.Entity<MsUser>().Update(user);

            var salt = Generator.GenerateSalt();

            if (user.UserPassword != null)
            {
                user.UserPassword.Salt = salt;
                user.UserPassword.HashedPassword = (body.NewPassword + salt).ToSHA512();
                _dbContext.Entity<MsUserPassword>().Update(user.UserPassword);
            }
            else
            {
                user.UserPassword = new MsUserPassword
                {
                    Id = user.Id,
                    Salt = salt,
                    HashedPassword = (body.NewPassword + salt).ToSHA512()
                };
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                if (user.UserSchools.Count != 0)
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(user.UserSchools.First().IdSchool, "USR10")
                    {
                        IdRecipients = new[] { user.Id },
                        KeyValues = new Dictionary<string, object>()
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
