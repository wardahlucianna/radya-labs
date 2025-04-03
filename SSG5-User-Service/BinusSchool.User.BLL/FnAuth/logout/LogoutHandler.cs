using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnAuth.Logout;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnAuth.Logout.Logout;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnAuth.Logout
{
    public class LogoutHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public LogoutHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<LogoutRequest, LogoutValidator>();

            var existToken = await _dbContext.Entity<MsUserPlatform>()
                .Where(x => x.FirebaseToken == body.FirebaseToken)
                .FirstOrDefaultAsync(CancellationToken);

            if (existToken != null)
            {
                existToken.IsActive = false;
                _dbContext.Entity<MsUserPlatform>().Update(existToken);
                
                await _dbContext.SaveChangesAsync(CancellationToken);
            }

            return Request.CreateApiResult2();
        }
    }
}
