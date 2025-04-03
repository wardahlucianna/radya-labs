using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.MenuAndPermission;
using Refit;

namespace BinusSchool.Data.Api.User.FnUser
{
    public interface IMenuAndPermission : IFnUser
    {
        [Get("/menu-permission")]
        Task<ApiErrorResult<IEnumerable<MenuAndPermissionResult>>> GetMenuAndPermissions();

        [Get("/menu-permission/user")]
        Task<ApiErrorResult<IEnumerable<UserMenuAndPermissionResult>>> GetUserMenuAndPermissions(GetUserMenuAndPermissionRequest request);

        [Get("/menu-permission/user-feature")]
        Task<ApiErrorResult<IEnumerable<FeatureUserMenuAndPermissionResult>>> GetFeatureUserMenuAndPermissions(GetFeatureUserMenuAndPermissionRequest request);
        [Get("/menu-permission/mobile-main-menu")]
        Task<ApiErrorResult<IEnumerable<GetMobileMainMenuResult>>> GetMobileMainMenu();
        [Get("/menu-permission/mobile-sub-menu")]
        Task<ApiErrorResult<IEnumerable<GetMobileSubMenuResult>>> GetMobileSubMenu(GetMobileSubMenuRequest request);
    }
}
