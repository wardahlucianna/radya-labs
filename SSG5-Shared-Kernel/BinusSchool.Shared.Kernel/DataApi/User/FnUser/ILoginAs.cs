using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.LoginAs.ImpersonateUser;
using BinusSchool.Data.Model.User.FnUser.LoginAs.ManageAccessRoleLoginAs;
using Refit;

namespace BinusSchool.Data.Api.User.FnUser
{
    public interface ILoginAs : IFnUser
    {
        [Get("/loginAs/GetGroupListAccessRoleLoginAs")]
        Task<ApiErrorResult<List<GetGroupListAccessRoleLoginAsResult>>> GetGroupListAccessRoleLoginAs(GetGroupListAccessRoleLoginAsRequest query);

        [Get("/loginAs/GetListAccessRoleLoginAs")]
        Task<ApiErrorResult<List<GetListAccessRoleLoginAsResult>>> GetListAccessRoleLoginAs(GetListAccessRoleLoginAsRequest query);

        [Post("/loginAs/AddUpdateAccessRoleLoginAs")]
        Task<ApiErrorResult> AddUpdateAccessRoleLoginAs([Body] AddUpdateAccessRoleLoginAsRequest query);

        [Get("/loginAs/GetListUserForLoginAs")]
        Task<ApiErrorResult<IEnumerable<GetListUserForLoginAsResult>>> GetListUserForLoginAs(GetListUserForLoginAsRequest query);

        [Post("/loginAs/ImpersonateUserLoginAs")]
        Task<ApiErrorResult<ImpersonateUserLoginAsResult>> ImpersonateUserLoginAs([Body] ImpersonateUserLoginAsRequest body);

        [Post("/loginAs/CheckUserNamePasswordImpersonateUserLoginAs")]
        Task<ApiErrorResult<CheckUserNamePasswordImpersonateUserLoginAsResult>> CheckUserNamePasswordImpersonateUserLoginAs([Body] CheckUserNamePasswordImpersonateUserLoginAsRequest body);
    }
}
