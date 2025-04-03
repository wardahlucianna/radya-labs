using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.Register;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.Register.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.Register
{
    public class RegisterPushTokenHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public RegisterPushTokenHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<RegisterPushTokenRequest, RegisterPushTokenValidator>();

            var existToken = await _dbContext.Entity<MsUserPlatform>()
                .Where(x => x.FirebaseToken == body.FirebaseToken)
                .FirstOrDefaultAsync(CancellationToken);

            if (existToken is null)
            {
                var newToken = new MsUserPlatform
                {
                    Id = Guid.NewGuid().ToString(),
                    IdUser = AuthInfo.UserId,
                    AppPlatform = body.Platform,
                    FirebaseToken = body.FirebaseToken
                };
                _dbContext.Entity<MsUserPlatform>().Add(newToken);
            }
            else
            {
                existToken.AppPlatform = body.Platform;
                _dbContext.Entity<MsUserPlatform>().Update(existToken);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
