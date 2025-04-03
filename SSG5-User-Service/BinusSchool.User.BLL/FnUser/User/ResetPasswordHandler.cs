using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.User.FnUser.User
{
    public class ResetPasswordHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        protected IDbContextTransaction Transaction;

        public ResetPasswordHandler(
            IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            if (!KeyValues.TryGetValue("idUser", out var idUser))
                throw new ArgumentNullException(nameof(idUser));

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var user = await _dbContext.Entity<MsUser>()
                                       .Include(x => x.UserPassword)
                                       .Where(x => x.Id == (string)idUser)
                                       .SingleOrDefaultAsync(CancellationToken);
            if (user is null)
                throw new NotFoundException("User is not found");
            if (user.IsActiveDirectory)
                throw new BadRequestException("Can't reset password user from active directory");

            var changePasswordCode = Guid.NewGuid().ToString();

            user.RequestChangePasswordCode = changePasswordCode;
            user.UsedDate = null;
            user.CountResetRequest = 0;
            _dbContext.Entity<MsUser>().Update(user);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "USR5")
                {
                    IdRecipients = new[] { user.Id },
                    KeyValues = new Dictionary<string, object>
                    {
                        { "receiverName", user.DisplayName },
                        { "username", user.Email },
                        { "email",user.Email},
                        { "passwordCode", changePasswordCode }
                    }
                });
                collector.Add(message);
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
