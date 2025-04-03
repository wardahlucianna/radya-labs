using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.User.FnUser.Role;
using Refit;

namespace BinusSchool.Data.Api.User.FnUser
{
    public interface IRole : IFnUser
    {
        [Get("/user/role/group")]
        Task<ApiErrorResult<IEnumerable<RoleResult>>> GetRoleGroups(CollectionRequest request);

        [Get("/user/role/group/by-code")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetRoleGroupByCode(IdCollection request);

        [Get("/user/role/group/by-id")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetRoleGroupById(IdCollection request);

        [Get("/user/role")]
        Task<ApiErrorResult<IEnumerable<RoleResult>>> GetRoles(GetRoleRequest request);

        [Get("/user/role/detail/{id}")]
        Task<ApiErrorResult<RoleDetailResult>> GetRoleDetail(string id);

        [Post("/user/role")]
        Task<ApiErrorResult> AddRole([Body] AddRoleRequest body);

        [Put("/user/role")]
        Task<ApiErrorResult> UpdateRole([Body] UpdateRoleRequest body);

        [Delete("/user/role")]
        Task<ApiErrorResult> DeleteRole([Body] IEnumerable<string> ids);

        [Get("/user/role/position")]
        Task<ApiErrorResult<IEnumerable<GetRolePositionHandlerResult>>> GetRolePosition(GetRolePositionRequest request);

        [Get("/user/role/by-idgroup")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetRoleByIdGroup(GetRoleByIdGroupRequest request);

        [Get("/user/role-staff")]
        Task<ApiErrorResult<IEnumerable<GetStaffRoleResult>>> GetStaffRoles(GetStaffRoleRequest request);
        [Get("/user/role/mobile-detail/{id}")]
        Task<ApiErrorResult<GetRoleMobileResult>> GetMobileRoleDetail(string id);
        [Get("/user/role/mobile-default-role")]
        Task<ApiErrorResult<GetMobileDefaultRoleResult>> GetMobileDefaultRoleDetail(GetMobileDefaultRoleRequest request);
        [Get("/user/role/role-type")]
        Task<ApiErrorResult<List<GetRoleMobileTypeResult>>> GetRoleMobileType(GetRoleMobileTypeRequest request);
    }
}
