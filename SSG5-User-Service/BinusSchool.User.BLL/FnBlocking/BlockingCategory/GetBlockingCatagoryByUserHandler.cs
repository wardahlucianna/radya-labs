using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.BlockingCategory;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnBlocking.BlockingCategory
{
    public class GetBlockingCatagoryByUserHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetBlockingCatagoryByUserHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetBlockingByUserRequest>();

            var query = _dbContext.Entity<MsUserBlocking>()
                .Include(x => x.BlockingCategory)
                .Where(x => x.IdUser == param.IdUser)
                .Select(x => new GetBlockingByUserResult
                {
                    BlockingCategoryId = x.IdBlockingCategory,
                    BlockingCategoryName = x.BlockingCategory.Name
                });

            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.BlockingCategoryName, param.SearchPattern()));
            }

            var result = await query
                .ToListAsync(CancellationToken);


            return Request.CreateApiResult2(result as object);
        }
    }
}
