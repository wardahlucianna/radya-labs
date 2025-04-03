using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.Role;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.Role
{
    public class GetRoleByIdGroupHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetRoleByIdGroupHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            
            var param = Request.ValidateParams<GetRoleByIdGroupRequest>(nameof(GetRoleByIdGroupRequest.IdSchool));

            var query = await _dbContext.Entity<LtRole>()
                                        .Include(x => x.RoleGroup)
                                        .Where(x => x.RoleGroup.Code == "PARENT"
                                        && x.IdSchool == param.IdSchool)
                                        .Select(x => new CodeWithIdVm(x.Id, x.Code, x.Description))
                                        .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
            
        }
    }
}
