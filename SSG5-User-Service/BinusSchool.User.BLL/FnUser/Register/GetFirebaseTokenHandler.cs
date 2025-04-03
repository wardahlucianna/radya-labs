using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.User.FnUser.Register;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.Register.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.Register
{
    public class GetFirebaseTokenHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetFirebaseTokenHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetFirebaseTokenRequest, GetFirebaseTokenRequestValidator>();

            if (body.AppPlatform == null)
                body.AppPlatform = new List<AppPlatform>();

            var predicate = PredicateBuilder.Create<MsUserPlatform>(x => body.IdUserRecipient.Contains(x.IdUser) && x.FirebaseToken != null);
            if (body.AppPlatform.Any())
                predicate = predicate.And(x => body.AppPlatform.Contains(x.AppPlatform));

            var tokens = await _dbContext.Entity<MsUserPlatform>()
                .Include(e => e.User)
               .Where(predicate)
               .Select(x => new GetFirebaseTokenResult
               {
                   IdUser = x.IdUser,
                   Token = x.FirebaseToken,
                   Name = x.User.DisplayName
               })
               .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(tokens as object);
        }
    }
}
