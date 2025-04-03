using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.User.FnUser.MenuAndPermission;
using BinusSchool.Data.Model.User.FnUser.Role;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.Utils;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.Role
{
    public class GetMobileDefaultRoleHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetMobileDefaultRoleHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMobileDefaultRoleRequest>(nameof(GetMobileDefaultRoleRequest.IdRoleGroup));

            if (param.IdRoleGroup != "TCH" && param.IdRoleGroup != "PRT" && param.IdRoleGroup != "STD")
                return Request.CreateApiResult2();

            var mandatoryRoleActions = new List<string>
            {
                MobileActionString.Dashboard,
                MobileActionString.Schedule,
                MobileActionString.MySchedule,
                MobileActionString.TeacherSchedule,
                MobileActionString.StudentTracking,
                MobileActionString.Attendance,
                MobileActionString.EmergencyAttendance,
                MobileActionString.Logout
            };

            if (param.IdRoleGroup == "TCH")
                mandatoryRoleActions.Remove(MobileActionString.Attendance);

            var features = await _dbContext.Entity<MsFeature>()
                .Include(x => x.FeaturePermissions).ThenInclude(x => x.Permission)
                .Where(x => mandatoryRoleActions.Contains(x.Action))
                .OrderBy(x => x.OrderNumber).ToListAsync(CancellationToken);

            if (features.Any())
            {
                var mobileFeatures = features.Where(x => string.IsNullOrEmpty(x.IdParent)).OrderBy(x => x.OrderNumber).Select(x => new GetMobileDefaultRoleResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    IdParent = x.IdParent,
                    Permissions = x.FeaturePermissions.Any() ? x.FeaturePermissions.Select(x => new CodeWithIdVm
                    {
                        Id = x.Id,
                        Code = x.Permission.Code,
                        Description = x.Permission.Description
                    }).ToList() : null,
                    Childs = GetChildsRecursion(x.Id, features)
                }).ToList();

                foreach (var mobileFeature in mobileFeatures)
                {
                    switch (mobileFeature.Id)
                    {
                        case "300":
                            mobileFeature.Type = RolePermissionType.BottomNavigation1.ToString();
                            break;
                        case "301":
                        case "305":
                        case "306":
                        case "307":
                            mobileFeature.Type = RolePermissionType.BottomNavigation2.ToString();
                            break;
                        case "302":
                            mobileFeature.Type = RolePermissionType.BottomNavigation3.ToString();
                            break;
                        case "303":
                        case "304":
                            mobileFeature.Type = RolePermissionType.MoreMenu.ToString();
                            break;
                        default:
                            break;
                    }
                }

                return Request.CreateApiResult2(mobileFeatures as object);
            }

            return Request.CreateApiResult2();
        }

        private List<GetMobileDefaultRoleResult> GetChildsRecursion(string idParent, List<MsFeature> features)
        {
            return features.Any(x => x.IdParent == idParent) ? features.Where(x => x.IdParent == idParent).OrderBy(x => x.OrderNumber).Select(x => new GetMobileDefaultRoleResult
            {
                Id = x.Id,
                Code = x.Code,
                Description = x.Description,
                IdParent = x.IdParent,
                Permissions = x.FeaturePermissions.Any() ? x.FeaturePermissions.Select(x => new CodeWithIdVm
                {
                    Id = x.Id,
                    Code = x.Permission.Code,
                    Description = x.Permission.Description
                }).ToList() : null,
                Childs = GetChildsRecursion(x.Id, features)
            }).ToList() : null;
        }
    }
}
