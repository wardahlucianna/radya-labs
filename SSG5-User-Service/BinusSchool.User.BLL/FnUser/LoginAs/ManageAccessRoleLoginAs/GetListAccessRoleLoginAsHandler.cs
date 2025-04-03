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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.LoginAs.ManageAccessRoleLoginAs
{
    public class GetListAccessRoleLoginAsHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly GetAllRoleDataForManageAccessRoleLoginAsHandler _getAllRoleDataForManageAccessRoleLoginAsHandler;

        public GetListAccessRoleLoginAsHandler(
            IUserDbContext dbContext,
            GetAllRoleDataForManageAccessRoleLoginAsHandler getAllRoleDataForManageAccessRoleLoginAsHandler
            )
        {
            _dbContext = dbContext;
            _getAllRoleDataForManageAccessRoleLoginAsHandler = getAllRoleDataForManageAccessRoleLoginAsHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListAccessRoleLoginAsRequest>(
                nameof(GetListAccessRoleLoginAsRequest.IdSchool)
                );

            var result = new List<GetListAccessRoleLoginAsResult>();

            var getAllRoleDataForManageAccessRoleLoginAs = await _getAllRoleDataForManageAccessRoleLoginAsHandler.GetAllRoleDataForManageAccessRoleLoginAs(new GetAllRoleDataForManageAccessRoleLoginAsRequest
            {
                IdSchool = param.IdSchool
            });

            var getRoleList = getAllRoleDataForManageAccessRoleLoginAs
                .Select(x => new
                {
                    x.IdRole,
                    x.RoleCode,
                    x.RoleDesc
                }).Distinct()
                .ToList();

            var getTotalRole = getRoleList.Count();

            foreach (var role in getRoleList)
            {
                var getTotalAccessRoles = getAllRoleDataForManageAccessRoleLoginAs
                    .Where(x => x.IdRole == role.IdRole && x.IdAuthorizedRole != null)
                    .Count();

                var setDataResult = new GetListAccessRoleLoginAsResult()
                {
                    Id = role.IdRole,
                    Code = role.RoleCode,
                    Description = role.RoleDesc,
                    AccessRole = new GetListAccessRoleLoginAsResult_AccessRole { 
                        Description = getTotalAccessRoles + " of " + getTotalRole,
                        AccessCount = getTotalAccessRoles
                    }
                };

                result.Add(setDataResult);
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
