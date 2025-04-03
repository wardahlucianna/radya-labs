using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnBlocking.StudentBlocking
{
    public class GetAccessBlockStudentBlockingHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetAccessBlockStudentBlockingHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAccessBlockStudentBlockingRequest>();
            var data = await _dbContext.Entity<MsUserBlocking>()
                            .Include(x => x.BlockingCategory)
                            .Where(x => x.IdUser == AuthInfo.UserId && x.BlockingCategory.IdSchool == param.IdSchool)
                            .ToListAsync(CancellationToken);

            var strJoin = string.Join(",", data.Select(x => x.BlockingCategory.Name));

            var query = new GetAccessBlockStudentBlockingResult
            {
                AccessToBlock = strJoin
            };

            return Request.CreateApiResult2(query as object);
        }
    }
}
