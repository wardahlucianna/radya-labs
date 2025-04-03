using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Api.Teaching.FnAssignment;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel.XML2008Type;
using BinusSchool.Data.Model.User.FnUser.MenuAndPermission;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.User.FnUser.MenuAndPermission
{
    public class GetUserMenuAndPermissionHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public GetUserMenuAndPermissionHandler(
            IUserDbContext dbContext,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();

            var param = Request.ValidateParams<GetUserMenuAndPermissionRequest>(nameof(GetUserMenuAndPermissionRequest.IdSchool),
                                                                                nameof(GetUserMenuAndPermissionRequest.IdUser),
                                                                                nameof(GetUserMenuAndPermissionRequest.IdRole));

            var userRoles = await _dbContext.Entity<MsUserRole>()
                .Where(x => x.IdUser == param.IdUser)
                .Select(x => new MsUserRole { IdRole = x.IdRole })
                .ToListAsync(CancellationToken);

            if (!userRoles.Any() || !userRoles.Any(x => x.IdRole == param.IdRole))
                throw new BadRequestException("You don't have access role");

            List<string> dataUserRole = new List<string>(); 

            dataUserRole.AddRange(userRoles.Select(x => x.IdRole));

            var roles = await _dbContext.Entity<LtRole>()
                                       .Include(x => x.RolePermissions).ThenInclude(x => x.FeaturePermission).ThenInclude(x => x.Feature)
                                       .Include(x => x.RolePermissions).ThenInclude(x => x.FeaturePermission).ThenInclude(x => x.Permission)
                                       .Include(x => x.RolePositions).ThenInclude(x => x.RolePositionPermissions).ThenInclude(x => x.FeaturePermission).ThenInclude(x => x.Feature)
                                       .Include(x => x.RolePositions).ThenInclude(x => x.RolePositionPermissions).ThenInclude(x => x.FeaturePermission).ThenInclude(x => x.Permission)
                                       .Where(x => dataUserRole.Any(y => y == x.Id))
                                       .ToListAsync(CancellationToken);
            if (roles is null)
                throw new BadRequestException("Role is not found");

            List<UserMenuAndPermissionResult> resultDatas = new List<UserMenuAndPermissionResult>();
            List<UserMenuAndPermissionResult> resultDataFix = new List<UserMenuAndPermissionResult>();

            foreach(var role in roles)
            {
                if (role.RolePositions.Any())
                {
                    var rolePositions = role.RolePositions.ToList();

                    if (!param.IsMobile)
                    {
                        // populate feature list
                        var features = rolePositions.SelectMany(x => x.RolePositionPermissions.Where(y => y.Type == RolePermissionType.Web.ToString()).Select(y => y.FeaturePermission.Feature)).Distinct().ToList();
                        if (!features.Any())
                            continue;

                        List<MsFeaturePermission> featurePermissions = new List<MsFeaturePermission>();
                        featurePermissions.AddRange(features.SelectMany(x => x.FeaturePermissions));

                        resultDatas = featurePermissions.Where(x => string.IsNullOrEmpty(x.Feature.IdParent) && !x.Feature.IsShowMobile).Select(x => new UserMenuAndPermissionResult
                        {
                            Id = x.Feature.Id,
                            Code = x.Feature.Code,
                            Description = x.Feature.Description,
                            IdParent = x.Feature.IdParent,
                            Controller = x.Feature.Controller,
                            Action = x.Feature.Action,
                            Icon = x.Feature.Icon,
                            ParamUrl = x.Feature.ParamUrl,
                            Permissions = x.Feature.FeaturePermissions.Any() ? x.Feature.FeaturePermissions.Select(x => new CodeWithIdVm
                            {
                                Id = x.IdFeature,
                                Code = x.Permission.Code,
                                Description = x.Permission.Description
                            }).ToList() : null,
                            Childs = GetChildsRecursion(x.Id, features),
                            OrderNumber = x.Feature.OrderNumber,
                        }).ToList();

                        foreach (var resultData in resultDatas)
                        {
                            if (resultDataFix.Any(x => x.Id == resultData.Id))
                            {
                                var resultDataFixById = resultDataFix.FirstOrDefault(x => x.Id == resultData.Id);
                                var permission = resultDataFixById.Permissions.ToList();
                                foreach (var resultDataPermission in resultData.Permissions)
                                {
                                    if (!permission.Any(x => x.Id == resultDataPermission.Id))  
                                        resultDataFix.FirstOrDefault(x => x.Id == resultData.Id).Permissions.Add(resultDataPermission);
                                    else
                                    {
                                        if (resultDataFixById.Childs == null)
                                            continue;

                                        var childResultDataFix = resultDataFixById.Childs.Select(e=>e.Id).ToList();
                                        var newChild = resultData.Childs.Where(e=> !childResultDataFix.Contains(e.Id)).ToList();
                                        resultDataFixById.Childs.AddRange(newChild);
                                    }
                                }
                            }
                            else
                            {
                                resultDataFix.Add(resultData);
                            }
                        }
                    }
                    else
                    {
                        // populate feature list
                        var mainFeatures = rolePositions.SelectMany(x => x.RolePositionPermissions.Where(y => y.Type != RolePermissionType.Web.ToString() && y.Type != RolePermissionType.MoreMenu.ToString())
                            .OrderBy(z => z.Type)
                            .Select(y => new
                            {
                                features = y.FeaturePermission.Feature,
                                type = y.Type,
                                idFeaturePermission = y.IdFeaturePermission,
                            }))
                            .Distinct()
                            .ToList();

                        if (!mainFeatures.Any())
                            continue;

                        List<MsFeaturePermission> featurePermissions = new List<MsFeaturePermission>();
                        featurePermissions.AddRange(mainFeatures.Select(x => x.features).SelectMany(x => x.FeaturePermissions));

                        resultDatas = featurePermissions.Where(x => string.IsNullOrEmpty(x.Feature.IdParent) && x.Feature.IsShowMobile).Select(x => new UserMenuAndPermissionResult
                        {
                            Id = x.Feature.Id,
                            Code = x.Feature.Code,
                            Description = x.Feature.Description,
                            IdParent = x.Feature.IdParent,
                            Controller = x.Feature.Controller,
                            Action = x.Feature.Action,
                            Icon = $"{_configuration.GetValue<string>("MobileMenuBlobUrl")}{x.Feature.Icon}",
                            ParamUrl = x.Feature.ParamUrl,
                            Permissions = x.Feature.FeaturePermissions.Any() ? x.Feature.FeaturePermissions.Select(x => new CodeWithIdVm
                            {
                                Id = x.IdFeature,
                                Code = x.Permission.Code,
                                Description = x.Permission.Description
                            }).ToList() : null,
                            Childs = GetChildsRecursion(x.Id, mainFeatures.Select(y => y.features).ToList()),
                            OrderNumber = x.Feature.OrderNumber,
                            Type = mainFeatures.Where(y => y.idFeaturePermission == x.Id).FirstOrDefault().type
                        }).ToList();

                        var moreMenuFeatures = rolePositions.SelectMany(x => x.RolePositionPermissions.Where(y => y.Type == RolePermissionType.MoreMenu.ToString()).Select(y => new
                        {
                            features = y.FeaturePermission.Feature,
                            type = y.Type,
                            idFeaturePermission = y.IdFeaturePermission
                        })).Distinct().ToList();

                        featurePermissions.Clear();
                        featurePermissions.AddRange(moreMenuFeatures.Select(x => x.features).SelectMany(x => x.FeaturePermissions));

                        var moreMenuDatas = featurePermissions.Where(x => string.IsNullOrEmpty(x.Feature.IdParent) && x.Feature.IsShowMobile).Select(x => new UserMenuAndPermissionResult
                        {
                            Id = x.Feature.Id,
                            Code = x.Feature.Code,
                            Description = x.Feature.Description,
                            IdParent = x.Feature.IdParent,
                            Controller = x.Feature.Controller,
                            Action = x.Feature.Action,
                            Icon = $"{_configuration.GetValue<string>("MobileMenuBlobUrl")}{x.Feature.Icon}",
                            ParamUrl = x.Feature.ParamUrl,
                            Permissions = x.Feature.FeaturePermissions.Any() ? x.Feature.FeaturePermissions.Select(x => new CodeWithIdVm
                            {
                                Id = x.IdFeature,
                                Code = x.Permission.Code,
                                Description = x.Permission.Description
                            }).ToList() : null,
                            Childs = GetChildsRecursion(x.Id, moreMenuFeatures.Select(y => y.features).ToList()),
                            OrderNumber = x.Feature.OrderNumber,
                            Type = moreMenuFeatures.Where(y => y.idFeaturePermission == x.Id).FirstOrDefault().type
                        }).ToList();

                        if (moreMenuDatas.Any())
                        {
                            var moreMenuData = resultDataFix.FirstOrDefault(x => x.Controller == "MobileMoreMenu");

                            if (moreMenuData == null)
                            {
                                resultDataFix.Add(new UserMenuAndPermissionResult
                                {
                                    Id = null,
                                    Code = "M-MM",
                                    Description = "More Menu",
                                    IdParent = null,
                                    Controller = "MobileMoreMenu",
                                    Action = "MobileMoreMenu",
                                    Icon = $"{_configuration.GetValue<string>("MobileMenuBlobUrl")}ic_more.png",
                                    ParamUrl = null,
                                    Permissions = null,
                                    Childs = moreMenuDatas,
                                    OrderNumber = int.MaxValue,
                                    Type = "MoreMenu"
                                });
                            }
                            else
                            {
                                //moreMenuData.Childs.AddRange(moreMenuDatas);
                                //moreMenuData.Childs.Distinct();
                                foreach (var recentChild in moreMenuDatas)
                                {
                                    if (!moreMenuData.Childs.Any(x => x.Controller == recentChild.Controller))
                                        moreMenuData.Childs.Add(recentChild);
                                }

                                int index = resultDataFix.FindIndex(x => x.Controller == "MobileMoreMenu");
                                if (index >= 0)
                                    resultDataFix[index].Childs = moreMenuData.Childs;
                            }
                        }

                        foreach (var resultData in resultDatas)
                        {
                            if (resultDataFix.Any(x => x.Id == resultData.Id))
                            {
                                var permission = resultDataFix.FirstOrDefault(x => x.Id == resultData.Id).Permissions.ToList();
                                foreach (var resultDataPermission in resultData.Permissions)
                                {
                                    if (!permission.Any(x => x.Id == resultDataPermission.Id))
                                        resultDataFix.FirstOrDefault(x => x.Id == resultData.Id).Permissions.Add(resultDataPermission);
                                }
                            }
                            else
                            {
                                resultDataFix.Add(resultData);
                            }
                        }
                    }
                }
                else if (role.RolePermissions.Any())
                {
                    if (!param.IsMobile)
                    {
                        // populate feature list
                        var features = role.RolePermissions.Where(x => x.Type == RolePermissionType.Web.ToString()).Select(x => x.FeaturePermission.Feature).Distinct().ToList();
                        if (!features.Any())
                            continue;

                        resultDatas = features.Where(x => string.IsNullOrEmpty(x.IdParent)).Select(x => new UserMenuAndPermissionResult
                        {
                            Id = x.Id,
                            Code = x.Code,
                            Description = x.Description,
                            IdParent = x.IdParent,
                            Controller = x.Controller,
                            Action = x.Action,
                            Icon = x.Icon,
                            ParamUrl = x.ParamUrl,
                            Permissions = x.FeaturePermissions.Where(y => y.Feature.IsShowMobile == param.IsMobile).Any() ?
                            x.FeaturePermissions.Where(y => y.Feature.IsShowMobile == param.IsMobile).Select(x => new CodeWithIdVm
                            {
                                Id = x.Id,
                                Code = x.Permission.Code,
                                Description = x.Permission.Description
                            }).ToList() : null,
                            Childs = GetChildsRecursion(x.Id, features),
                            OrderNumber = x.OrderNumber,
                        }).ToList();

                        foreach (var resultData in resultDatas)
                        {
                            if (resultDataFix.Any(x => x.Id == resultData.Id))
                            {
                                var resultDataFixById = resultDataFix.FirstOrDefault(x => x.Id == resultData.Id);
                                var permission = resultDataFixById.Permissions.ToList();
                                foreach (var resultDataPermission in resultData.Permissions)
                                {
                                    if (!permission.Any(x => x.Id == resultDataPermission.Id))
                                        resultDataFix.FirstOrDefault(x => x.Id == resultData.Id).Permissions.Add(resultDataPermission);
                                    else
                                    {
                                        if (resultDataFixById.Childs == null)
                                            continue;
                                        var childResultDataFix = resultDataFixById.Childs.Select(e => e.Id).ToList();
                                        var newChild = resultData.Childs.Where(e => !childResultDataFix.Contains(e.Id)).ToList();
                                        resultDataFixById.Childs.AddRange(newChild);
                                    }
                                }
                            }
                            else
                            {
                                resultDataFix.Add(resultData);
                            }
                        }
                    }
                    else
                    {
                        // populate feature list
                        var mainFeatures = role.RolePermissions.Where(x => x.Type != RolePermissionType.Web.ToString() && x.Type != RolePermissionType.MoreMenu.ToString())
                            .OrderBy(z => z.Type)
                            .Select(y => new
                            {
                                features = y.FeaturePermission.Feature,
                                type = y.Type,
                                idFeaturePermission = y.IdFeaturePermission,
                            })
                            .Distinct()
                            .ToList();

                        if (!mainFeatures.Any())
                            continue;

                        resultDatas = mainFeatures.Where(x => string.IsNullOrEmpty(x.features.IdParent)).Select(x => new UserMenuAndPermissionResult
                        {
                            Id = x.features.Id,
                            Code = x.features.Code,
                            Description = x.features.Description,
                            IdParent = x.features.IdParent,
                            Controller = x.features.Controller,
                            Action = x.features.Action,
                            Icon = $"{_configuration.GetValue<string>("MobileMenuBlobUrl")}{x.features.Icon}",
                            ParamUrl = x.features.ParamUrl,
                            Permissions = x.features.FeaturePermissions.Where(y => y.Feature.IsShowMobile == param.IsMobile).Any() ?
                            x.features.FeaturePermissions.Where(y => y.Feature.IsShowMobile == param.IsMobile).Select(x => new CodeWithIdVm
                            {
                                Id = x.Id,
                                Code = x.Permission.Code,
                                Description = x.Permission.Description
                            }).ToList() : null,
                            Childs = GetChildsRecursion(x.features.Id, mainFeatures.Select(y => y.features).ToList()),
                            OrderNumber = x.features.OrderNumber,
                            Type = x.features.FeaturePermissions.FirstOrDefault(y => y.Id == x.features.Id)
                                .RolePermissions.Where(y => y.IdFeaturePermission == x.features.Id && y.IdRole == role.Id).FirstOrDefault().Type
                        }).ToList();

                        var moreMenuFeatures = role.RolePermissions.Where(x => x.Type == RolePermissionType.MoreMenu.ToString()).Select(y => new
                        {
                            features = y.FeaturePermission.Feature,
                            type = y.Type,
                            idFeaturePermission = y.IdFeaturePermission,
                        }).Distinct().ToList();

                        var moreMenuDatas = moreMenuFeatures.Where(x => string.IsNullOrEmpty(x.features.IdParent)).Select(x => new UserMenuAndPermissionResult
                        {
                            Id = x.features.Id,
                            Code = x.features.Code,
                            Description = x.features.Description,
                            IdParent = x.features.IdParent,
                            Controller = x.features.Controller,
                            Action = x.features.Action,
                            Icon = $"{_configuration.GetValue<string>("MobileMenuBlobUrl")}{x.features.Icon}",
                            ParamUrl = x.features.ParamUrl,
                            Permissions = x.features.FeaturePermissions.Where(y => y.Feature.IsShowMobile == param.IsMobile).Any() ?
                            x.features.FeaturePermissions.Where(y => y.Feature.IsShowMobile == param.IsMobile).Select(x => new CodeWithIdVm
                            {
                                Id = x.Id,
                                Code = x.Permission.Code,
                                Description = x.Permission.Description
                            }).ToList() : null,
                            Childs = GetChildsRecursion(x.features.Id, moreMenuFeatures.Select(y => y.features).ToList()),
                            OrderNumber = x.features.OrderNumber,
                            Type = x.features.FeaturePermissions.FirstOrDefault(y => y.Id == x.features.Id)
                                .RolePermissions.Where(y => y.IdFeaturePermission == x.features.Id && y.IdRole == role.Id).FirstOrDefault().Type
                        }).ToList();

                        if (moreMenuDatas.Any())
                        {
                            var moreMenuData = resultDataFix.FirstOrDefault(x => x.Controller == "MobileMoreMenu");

                            if (moreMenuData == null)
                            {
                                resultDataFix.Add(new UserMenuAndPermissionResult
                                {
                                    Id = null,
                                    Code = "M-MM",
                                    Description = "More Menu",
                                    IdParent = null,
                                    Controller = "MobileMoreMenu",
                                    Action = "MobileMoreMenu",
                                    Icon = $"{_configuration.GetValue<string>("MobileMenuBlobUrl")}ic_more.png",
                                    ParamUrl = null,
                                    Permissions = null,
                                    Childs = moreMenuDatas,
                                    OrderNumber = int.MaxValue,
                                    Type = "MoreMenu"
                                });
                            }
                            else
                            {
                                foreach (var recentChild in moreMenuDatas)
                                {
                                    if (!moreMenuData.Childs.Any(x => x.Controller == recentChild.Controller))
                                        moreMenuData.Childs.Add(recentChild);
                                }

                                int index = resultDataFix.FindIndex(x => x.Controller == "MobileMoreMenu");
                                if (index >= 0)
                                    resultDataFix[index].Childs = moreMenuData.Childs;
                            }
                        }

                        foreach (var resultData in resultDatas)
                        {
                            if (resultDataFix.Any(x => x.Id == resultData.Id))
                            {
                                var permission = resultDataFix.FirstOrDefault(x => x.Id == resultData.Id).Permissions.ToList();
                                foreach (var resultDataPermission in resultData.Permissions)
                                {
                                    if (!permission.Any(x => x.Id == resultDataPermission.Id))
                                        resultDataFix.FirstOrDefault(x => x.Id == resultData.Id).Permissions.Add(resultDataPermission);
                                }
                            }
                            else
                            {
                                resultDataFix.Add(resultData);
                            }
                        }
                    }
                }
            }

            // Ordering Childs
            foreach (var resultDataFixOrdering in resultDataFix.Where(x => x.Childs != null).ToList())
            {
                var orderedChilds = resultDataFixOrdering.Childs.OrderBy(x => x.OrderNumber).ToList();

                int index = resultDataFix.FindIndex(x => x.Controller == resultDataFixOrdering.Controller);
                if (index >= 0)
                    resultDataFix[index].Childs = orderedChilds;
            }

            return Request.CreateApiResult2(resultDataFix.Distinct().OrderBy(x => x.Type).ToList() as object);
        }

        private List<UserMenuAndPermissionResult> GetChildsRecursion(string idParent, List<MsFeature> features)
        {
            List<MsFeature> listFeatures = new List<MsFeature>();
            List<string> IdFeature = new List<string>();
            foreach (var datafix in features)
            {
                if (!IdFeature.Contains(datafix.Id))
                {
                    listFeatures.Add(datafix);
                    IdFeature.Add(datafix.Id);
                }

            }
            listFeatures = listFeatures.Distinct().ToList();
            return listFeatures.Any(x => x.IdParent == idParent) ? listFeatures.Where(x => x.IdParent == idParent).OrderBy(x => x.OrderNumber).Select(x => new UserMenuAndPermissionResult
            {
                Id = x.Id,
                Code = x.Code,
                Description = x.Description,
                IdParent = x.IdParent,
                Controller = x.Controller,
                Action = x.Action,
                Icon = x.Icon,
                ParamUrl = x.ParamUrl,
                Permissions = x.FeaturePermissions.Any() ? x.FeaturePermissions.Select(x => new CodeWithIdVm
                {
                    Id = x.Id,
                    Code = x.Permission.Code,
                    Description = x.Permission.Description
                }).ToList() : null,
                Childs = GetChildsRecursion(x.Id, features),
                OrderNumber = x.OrderNumber
            }).ToList() : null;
        }
    }
}
