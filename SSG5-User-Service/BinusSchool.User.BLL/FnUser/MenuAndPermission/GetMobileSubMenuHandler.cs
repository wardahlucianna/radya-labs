using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.MenuAndPermission;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.MenuAndPermission
{
    public class GetMobileSubMenuHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetMobileSubMenuHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMobileSubMenuRequest>(nameof(GetMobileSubMenuRequest.Id));

            return Request.CreateApiResult2(await _dbContext.Entity<MsFeature>().Where(x => x.IdParent == param.Id && x.IsShowMobile).Select(x => new GetMobileSubMenuResult
            {
                Id = x.Id,
                Code = x.Code,
                Description = x.Description
            }).ToListAsync(CancellationToken) as object);
        }
    }
}
