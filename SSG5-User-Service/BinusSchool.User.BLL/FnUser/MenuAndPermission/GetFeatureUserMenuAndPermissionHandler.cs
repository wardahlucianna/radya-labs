using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.User.FnUser.MenuAndPermission;
using BinusSchool.Data.Model.User.FnUser.Role;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.MenuAndPermission
{
    public class GetFeatureUserMenuAndPermissionHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetFeatureUserMenuAndPermissionHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUserMenuAndPermissionRequest>(nameof(GetUserMenuAndPermissionRequest.IdRole));

            var DataFeature = await _dbContext.Entity<MsFeature>()
                           .Select(x => new FeatureUserMenuAndPermissionResult
                           {
                               Action = "Action",
                               Controller = "Controller",
                               Icon = null,
                               Submenus = null,
                               Title = x.Description,
                               Url = "Controller/Action"
                           })
                           .ToListAsync();

            if (param.IdRole != "SADM")
            {
                var DataPermissionIds = await _dbContext.Entity<LtRole>()
                           .Include(x => x.RolePermissions).ThenInclude(e => e.FeaturePermission)
                           .Where(x => x.Id == param.IdRole)
                           .Select(x => new RoleDetailResult
                           {
                               //WebPermissionIds = x.RolePermissions.Where(y => y.Type == RolePermissionType.Web.ToString()).Any() ? x.RolePermissions.Where(y => y.Type == RolePermissionType.Web.ToString()).Select(y => y.IdFeaturePermission).ToList() : null,
                               //MobilePermissionIds = x.RolePermissions.Where(y => y.Type != RolePermissionType.Web.ToString()).Any() ? x.RolePermissions.Where(y => y.Type != RolePermissionType.Web.ToString()).Select(y => new MobilePermissionRequest
                               PermissionIds = x.RolePermissions.Where(y => y.Type == RolePermissionType.Web.ToString()).Any() ? x.RolePermissions.Where(y => y.Type == RolePermissionType.Web.ToString()).Select(y => y.IdFeaturePermission).ToList() : null,
                           }).SingleOrDefaultAsync();

                if (DataPermissionIds is null)
                    throw new BadRequestException("Feature is not found");

                var dataIdFeaturePermission = DataPermissionIds.PermissionIds;
                DataFeature = await _dbContext.Entity<MsFeaturePermission>()
                                .Include(x => x.Feature)
                                .Where(x => dataIdFeaturePermission.Contains(x.IdFeature))
                                .Select(x => new FeatureUserMenuAndPermissionResult
                                {
                                    Action = "Action",
                                    Controller = "Controller",
                                    Icon = null,
                                    Submenus = null,
                                    Title = x.Feature.Description,
                                    Url = "Controller/Action"
                                })
                                .ToListAsync();
            }
            
            return Request.CreateApiResult2(DataFeature as object);
        }
    }
}
