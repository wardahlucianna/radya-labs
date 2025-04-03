using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Employee.FnStaff;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.User
{
    public class UserBySchoolAndRoleHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        public UserBySchoolAndRoleHandler(IUserDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUserBySchoolAndRoleRequest>(nameof(GetUserBySchoolAndRoleRequest.IdSchool), nameof(GetUserBySchoolAndRoleRequest.IdRole));
            var predicate = PredicateBuilder.Create<MsUser>(p => p.UserRoles.Any(x => x.Role.RoleGroup.Id == param.IdRole) && p.UserSchools.Any(x => x.IdSchool == param.IdSchool));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.DisplayName, $"%{param.Search}%")
                    || EF.Functions.Like(x.Username, $"%{param.Search}%"));

            var users = await _dbContext.Entity<MsUser>()
                .Include(p => p.UserRoles).ThenInclude(p => p.Role)
                .Include(p => p.UserSchools)
                .Where(predicate)
                .Select(p => new GetUserResult
                {
                    Id = p.Id,
                    DisplayName = p.DisplayName,
                    Description = p.DisplayName
                })
                .ToListAsync(CancellationToken);

            users = users.Select(x => new GetUserResult
            {
                Id = x.Id,
                DisplayName = x.DisplayName.Trim(),
                Description = x.DisplayName.Trim()
            }).OrderBy(x => x.DisplayName).ToList();

            users =
            (
                from _users in users
                join _usersProfile in _dbContext.Entity<MsStaff>() on _users.Id equals _usersProfile.IdBinusian
                select new GetUserResult
                {
                    Id = _users.Id,
                    DisplayName = _users.DisplayName,
                    Description = _users.DisplayName,
                    Code = _usersProfile.InitialName,
                    ShortName = _usersProfile.ShortName
                }
            ).ToList();

            return Request.CreateApiResult2(users as object);
        }
    }
}
