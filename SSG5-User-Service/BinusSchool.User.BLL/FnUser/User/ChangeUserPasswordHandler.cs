using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class ChangeUserPasswordHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        protected IDbContextTransaction Transaction;

        public ChangeUserPasswordHandler(
            IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<ChangeUserPasswordRequest, ChangeUserPasswordValidator>();
            if (!body.NewPassword.ValidatePassword())
                throw new BadRequestException("Password must be at least 8 character with combination of uppercase and lowercase characters and number");

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var user = await _dbContext.Entity<MsUser>()
                                       .Include(x => x.UserPassword)
                                       .Where(x => x.RequestChangePasswordCode == body.UserId)
                                       .SingleOrDefaultAsync();
            if (user is null)
                throw new NotFoundException("User is not found");
            if (user.UserPassword.HashedPassword == (body.NewPassword + user.UserPassword.Salt).ToSHA512())
                throw new BadRequestException("Your new password must be different with your previous password");

            var salt = Generator.GenerateSalt();

            user.UserPassword.Salt = salt;
            user.UserPassword.HashedPassword = (body.NewPassword + salt).ToSHA512();
            _dbContext.Entity<MsUserPassword>().Update(user.UserPassword);

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
