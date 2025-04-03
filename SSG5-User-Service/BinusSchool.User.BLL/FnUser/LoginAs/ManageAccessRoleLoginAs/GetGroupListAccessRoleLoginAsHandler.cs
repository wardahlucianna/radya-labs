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
    public class GetGroupListAccessRoleLoginAsHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly GetAllRoleDataForManageAccessRoleLoginAsHandler _getAllRoleDataForManageAccessRoleLoginAsHandler;

        public GetGroupListAccessRoleLoginAsHandler(
            IUserDbContext dbContext,
            GetAllRoleDataForManageAccessRoleLoginAsHandler getAllRoleDataForManageAccessRoleLoginAsHandler
            )
        {
            _dbContext = dbContext;
            _getAllRoleDataForManageAccessRoleLoginAsHandler = getAllRoleDataForManageAccessRoleLoginAsHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGroupListAccessRoleLoginAsRequest>(
                nameof(GetGroupListAccessRoleLoginAsRequest.IdSchool),
                nameof(GetGroupListAccessRoleLoginAsRequest.IdRole)
                );

            var result = new List<GetGroupListAccessRoleLoginAsResult>();

            var getAllRoleDataForManageAccessRoleLoginAs = await _getAllRoleDataForManageAccessRoleLoginAsHandler.GetAllRoleDataForManageAccessRoleLoginAs(new GetAllRoleDataForManageAccessRoleLoginAsRequest
            {
                IdSchool = param.IdSchool
            });

            var getRoleGroupList = getAllRoleDataForManageAccessRoleLoginAs
                   .Select(x => new 
                   {
                       Id = x.IdRoleGroup,
                       Code = x.RoleGroupCode,
                       Description = x.RoleGroupDesc
                   }).Distinct()
                   .ToList();

            var IdAuthorizedRole = getAllRoleDataForManageAccessRoleLoginAs
                   .Where(x => x.IdRole == param.IdRole)
                   .Select(x => x.IdAuthorizedRole)
                   .Distinct().ToList();

            foreach (var roleGroup in getRoleGroupList)
            {
                var getRoleListChecked = new List<GetGroupListAccessRoleLoginAsResult_Role>();

                var getRoleList = getAllRoleDataForManageAccessRoleLoginAs
                    .Where(x => x.IdRoleGroup == roleGroup.Id)
                    .Select(x => new
                    {
                        Id = x.IdRole,
                        Code = x.RoleCode,
                        Description = x.RoleDesc
                    }).Distinct()
                    .OrderBy(x => x.Description)
                    .ToList();

                foreach (var role in getRoleList)
                {
                    var IsRoleChecked = IdAuthorizedRole.Any(x => x == role.Id);

                    var roleChecked =  new GetGroupListAccessRoleLoginAsResult_Role
                        {
                            Id = role.Id,
                            Code = role.Code,
                            Description = role.Description,
                            IsRoleChecked = IsRoleChecked
                        };
                    getRoleListChecked.Add(roleChecked);
                }

                var setDataResult = new GetGroupListAccessRoleLoginAsResult()
                {
                    RoleGroup = new CodeWithIdVm
                    {
                        Id = roleGroup.Id,
                        Code = roleGroup.Code,
                        Description = roleGroup.Description
                    },
                    RoleList = getRoleListChecked,
                    CountRoleList = getRoleListChecked.Count()
                };

                result.Add(setDataResult);
            }

            return Request.CreateApiResult2(result.OrderBy(x => x.CountRoleList).ThenBy(x => x.RoleGroup.Description) as object);
        }
    }
}
