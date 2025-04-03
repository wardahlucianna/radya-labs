using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.LoginAs.ManageAccessRoleLoginAs;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.LoginAs.ManageAccessRoleLoginAs
{
    public class GetAllRoleDataForManageAccessRoleLoginAsHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetAllRoleDataForManageAccessRoleLoginAsHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAllRoleDataForManageAccessRoleLoginAsRequest>(nameof(GetAllRoleDataForManageAccessRoleLoginAsRequest.IdSchool));

            var result = await GetAllRoleDataForManageAccessRoleLoginAs(new GetAllRoleDataForManageAccessRoleLoginAsRequest
            {
                IdSchool = param.IdSchool
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<List<GetAllRoleDataForManageAccessRoleLoginAsResult>> GetAllRoleDataForManageAccessRoleLoginAs(GetAllRoleDataForManageAccessRoleLoginAsRequest param)
        {
            var getListRole = await _dbContext.Entity<LtRole>()
                    .Where(x => x.IdSchool == param.IdSchool)
                    .Select(x => new {
                        IdSchool = x.IdSchool,
                        IdRoleGroup = x.RoleGroup.Id,
                        RoleGroupCode = x.RoleGroup.Code,
                        RoleGroupDesc = x.RoleGroup.Description,
                        IdRole = x.Id,
                        RoleCode = x.Code,
                        RoleDesc = x.Description
                    }).Distinct()
                    .ToListAsync(CancellationToken);

            var getAllRoleLoginAs = await _dbContext.Entity<TrRoleLoginAs>()
                    .Where(x => x.Role.IdSchool == param.IdSchool && x.AuthorizedRole.IdSchool == param.IdSchool)
                    .Select(x => new
                    {
                        IdRole = x.IdRole,
                        IdAuthorizedRole = x.IdAuthorizedRole,
                        AuthorizedRoleCode = x.AuthorizedRole.Code,
                        AuthorizedRoleDesc = x.AuthorizedRole.Description,
                        AuthorizedRoleGroupCode = x.AuthorizedRole.RoleGroup.Code,
                        AuthorizedRoleGroupDesc = x.AuthorizedRole.RoleGroup.Description
                    }).Distinct()
                    .ToListAsync(CancellationToken);

            var getAllRoleDataForManageAccessRoleLoginAs = getListRole
                    .GroupJoin(getAllRoleLoginAs,
                        role => (role.IdRole),
                        accessRole => (accessRole.IdRole),
                        (role, accessRole) => new { role, accessRole }
                    ).SelectMany(x => x.accessRole.DefaultIfEmpty(),
                    (roles, accessRoles) => new GetAllRoleDataForManageAccessRoleLoginAsResult
                    {
                        IdSchool = roles.role.IdSchool,
                        IdRoleGroup = roles.role.IdRoleGroup,
                        RoleGroupCode = roles.role.RoleGroupCode,
                        RoleGroupDesc = roles.role.RoleGroupDesc,
                        IdRole = roles.role.IdRole,
                        RoleCode = roles.role.RoleCode,
                        RoleDesc = roles.role.RoleDesc,
                        IdAuthorizedRole = accessRoles == null ? null : accessRoles.IdAuthorizedRole,
                        AuthorizedRoleCode = accessRoles == null ? null : accessRoles.AuthorizedRoleCode,
                        AuthorizedRoleDesc = accessRoles == null ? null : accessRoles.AuthorizedRoleDesc,
                        AuthorizedRoleGroupCode = accessRoles == null ? null : accessRoles.AuthorizedRoleGroupCode,
                        AuthorizedRoleGroupDesc = accessRoles == null ? null : accessRoles.AuthorizedRoleGroupDesc
                    }).Distinct().ToList();

            return getAllRoleDataForManageAccessRoleLoginAs;
        }
    }
}
