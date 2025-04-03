using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.MenuAndPermission;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.MenuAndPermission
{
    public class GetMobileMainMenuHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetMobileMainMenuHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var features = await _dbContext.Entity<MsFeature>()
                               .Include(x => x.FeaturePermissions).ThenInclude(x => x.Permission)
                               .Where(x => x.IsShowMobile == true)
                               .ToListAsync();

            if (features.Any())
            {
                var mobileFeatures = features.Where(x => string.IsNullOrEmpty(x.IdParent)).OrderBy(x => x.OrderNumber).Select(x => new MenuAndPermissionResult
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

                return Request.CreateApiResult2(mobileFeatures as object);
            }

            return Request.CreateApiResult2();
        }

        private List<MenuAndPermissionResult> GetChildsRecursion(string idParent, List<MsFeature> features)
        {
            return features.Any(x => x.IdParent == idParent) ? features.Where(x => x.IdParent == idParent).OrderBy(x => x.OrderNumber).Select(x => new MenuAndPermissionResult
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
