using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.Utils;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.User
{
    public class GenerateUsernameHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        public GenerateUsernameHandler(IUserDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GenerateUsernameRequest>(nameof(GenerateUsernameRequest.IdRole));

            var role = await _dbContext.Entity<LtRole>()
                                       .Include(x => x.RoleSettings)
                                       .SingleOrDefaultAsync(x => x.Id == param.IdRole);
            if (role is null)
                throw new NotFoundException("Role not found");

            return role.RoleSettings.Any(x => x.IsArrangeUsernameFormat && !string.IsNullOrEmpty(x.UsernameFormat))
                ? Request.CreateApiResult2(role.RoleSettings.First(x => x.IsArrangeUsernameFormat && !string.IsNullOrEmpty(x.UsernameFormat))
                                                            .UsernameFormat.GenerateUsername((await _dbContext.Entity<MsUserRole>()
                                                                                                              .CountAsync(x => x.IdRole == param.IdRole)) + 1, param.BinusianIds) as object)
                : Request.CreateApiResult2(null as object);
        }
    }
}
