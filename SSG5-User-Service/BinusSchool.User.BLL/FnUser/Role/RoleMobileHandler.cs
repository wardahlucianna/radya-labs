using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.User.FnUser.MenuAndPermission;
using BinusSchool.Data.Model.User.FnUser.Role;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.Role
{
    public class RoleMobileHandler : FunctionsHttpCrudHandler
    {
        private readonly IUserDbContext _dbContext;

        public RoleMobileHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var role = await _dbContext.Entity<LtRole>()
                .Include(x => x.RoleGroup)
                .Include(x => x.RoleSettings)
                .Include(x => x.RolePermissions)
                    .ThenInclude(x => x.FeaturePermission)
                        .ThenInclude(x => x.Feature)
                .Include(x => x.RolePositions)
                    .ThenInclude(x => x.TeacherPosition)
                .Include(x => x.RolePositions)
                    .ThenInclude(x => x.RolePositionPermissions)
                        .ThenInclude(x => x.FeaturePermission)
                            .ThenInclude(x => x.Feature)
                .Where(x => x.Id == id).FirstOrDefaultAsync(CancellationToken);

            if (role is null)
                throw new NotFoundException("Role is not found");

            var result = new GetRoleMobileResult
            {
                Id = role.Id,
                Code = role.Code,
                Description = role.Description,
                PermissionIds = role.RolePermissions != null ?
                    role.RolePermissions.Where(y => y.Type != RolePermissionType.Web.ToString() && y.FeaturePermission.Feature.IdParent == null).Select(y => new MobilePermissionResult
                    {
                        Id = y.IdFeaturePermission,
                        Type = y.Type,
                        Description = y.FeaturePermission.Feature.Description,
                        FeatureOrderNumber = y.FeaturePermission.Feature.OrderNumber,
                        Childs = GetChildsRecursion(y.FeaturePermission.IdFeature, role.RolePermissions.ToList())
                    }).OrderBy(z => z.FeatureOrderNumber).ToList() : null,
                RolePositions = role.RolePositions.Any() ? role.RolePositions.Select(y => new MobileRolePositionResult
                {
                    Id = y.Id,
                    TeacherPosition = new CodeWithIdVm
                    {
                        Id = y.TeacherPosition.Id,
                        Code = y.TeacherPosition.Code,
                        Description = y.TeacherPosition.Description
                    },
                    PermissionIds = y.RolePositionPermissions != null ?
                    y.RolePositionPermissions.Where(z => z.Type != RolePermissionType.Web.ToString() && z.FeaturePermission.Feature.IdParent == null).Select(z => new MobilePermissionResult
                    {
                        Id = z.IdFeaturePermission,
                        Type = z.Type,
                        Description = z.FeaturePermission.Feature.Description,
                        FeatureOrderNumber = z.FeaturePermission.Feature.OrderNumber,
                        Childs = GetPositionChildsRecursion(z.FeaturePermission.IdFeature, y.RolePositionPermissions.ToList())
                    }).OrderBy(z => z.FeatureOrderNumber).ToList() : null
                }).ToList() : null
            };

            if (result is null)
                throw new NotFoundException("Role is not found");

            return Request.CreateApiResult2(result as object);
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new System.NotImplementedException();
        }

        private List<MobilePermissionResult> GetChildsRecursion(string idParent, List<TrRolePermission> rolePermissions)
        {
            return rolePermissions.Any(x => x.FeaturePermission.Feature.IdParent == idParent) ? rolePermissions.Where(x => x.FeaturePermission.Feature.IdParent == idParent)
                .OrderBy(x => x.FeaturePermission.Feature.OrderNumber).Select(x => new MobilePermissionResult
                {
                    Id = x.IdFeaturePermission,
                    Type = x.Type,
                    Description = x.FeaturePermission.Feature.Description,
                    FeatureOrderNumber = x.FeaturePermission.Feature.OrderNumber,
                    Childs = GetChildsRecursion(x.Id, rolePermissions)
                }).ToList() : null;
        }

        private List<MobilePermissionResult> GetPositionChildsRecursion(string idParent, List<TrRolePositionPermission> rolePositionPermissions)
        {
            return rolePositionPermissions.Any(x => x.FeaturePermission.Feature.IdParent == idParent) ? rolePositionPermissions.Where(x => x.FeaturePermission.Feature.IdParent == idParent)
                .OrderBy(x => x.FeaturePermission.Feature.OrderNumber).Select(x => new MobilePermissionResult
                {
                    Id = x.IdFeaturePermission,
                    Type = x.Type,
                    Description = x.FeaturePermission.Feature.Description,
                    FeatureOrderNumber = x.FeaturePermission.Feature.OrderNumber,
                    Childs = GetPositionChildsRecursion(x.Id, rolePositionPermissions)
                }).ToList() : null;
        }
    }
}
