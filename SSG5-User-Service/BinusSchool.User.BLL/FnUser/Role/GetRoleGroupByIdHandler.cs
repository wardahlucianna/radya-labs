using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.Role
{
    public class GetRoleGroupByIdHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetRoleGroupByIdHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<IdCollection>(nameof(IdCollection.Ids));

            var query = await _dbContext.Entity<LtRoleGroup>()
                .Where(x => param.Ids.Contains(x.Id))
                .Select(x => new CodeWithIdVm(x.Id, x.Code, x.Description))
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }
    }
}
