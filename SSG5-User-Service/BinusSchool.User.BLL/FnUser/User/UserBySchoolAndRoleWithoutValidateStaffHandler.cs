using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.User
{
    public class UserBySchoolAndRoleWithoutValidateStaffHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        public UserBySchoolAndRoleWithoutValidateStaffHandler(IUserDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUserBySchoolAndRoleRequest>(nameof(GetUserBySchoolAndRoleRequest.IdSchool), nameof(GetUserBySchoolAndRoleRequest.IdRole));

            var users = await _dbContext.Entity<MsUser>()
                .Include(p => p.UserRoles).ThenInclude(p => p.Role)
                .Include(p => p.UserSchools)
                .Where(p => p.UserRoles.Any(x => x.Role.IdRoleGroup == param.IdRole) && p.UserSchools.Any(x => x.IdSchool == param.IdSchool))
                .Select(p => new GetUserResult
                {
                    Id = p.Id,
                    DisplayName = p.DisplayName,
                    Description = p.DisplayName
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(users as object);
        }
    }
}
