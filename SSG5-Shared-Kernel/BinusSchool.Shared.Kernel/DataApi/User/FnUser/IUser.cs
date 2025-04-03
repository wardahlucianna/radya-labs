using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.User;
using Refit;

namespace BinusSchool.Data.Api.User.FnUser
{
    public interface IUser : IFnUser
    {
        [Get("/user")]
        Task<ApiErrorResult<IEnumerable<ListUserResult>>> GetUsers(GetUserRequest request);

        [Get("/user/detail/{id}")]
        Task<ApiErrorResult<GetUserDetailResult>> GetUserDetail(string id);

        [Get("/user-by-school-role")]
        Task<ApiErrorResult<IEnumerable<GetUserResult>>> GetUserByRoleAndSchool(GetUserBySchoolAndRoleRequest request);

        [Get("/user-by-school-role-without-validate-staff")]
        Task<ApiErrorResult<IEnumerable<GetUserResult>>> GetUserByRoleAndSchoolWithoutValidateStaff(GetUserBySchoolAndRoleRequest request);

        [Get("/generate-username")]
        Task<ApiErrorResult<string>> GenerateUsername(GenerateUsernameRequest request);

        [Post("/user")]
        Task<ApiErrorResult> AddUser([Body] AddUserRequest body);

        [Put("/user")]
        Task<ApiErrorResult> UpdateUser([Body] UpdateUserRequest body);

        [Delete("/user")]
        Task<ApiErrorResult> DeleteUser([Body] IEnumerable<string> ids);

        [Put("/user/set-status")]
        Task<ApiErrorResult> SetStatusUser([Body] SetStatusUserRequest body);

        [Put("/user/reset-password/{idUser}")]
        Task<ApiErrorResult> ResetPassword(string idUser);

        [Put("/user/change-password")]
        Task<ApiErrorResult> ChangePassword([Body] ChangePasswordRequest body);

        [Put("/user/change-user-password")]
        Task<ApiErrorResult> ChangeUserPassword([Body] ChangeUserPasswordRequest body);

        [Post("/user/forgot-password")]
        Task<ApiErrorResult> ForgotPassword([Body] ForgotPasswordRequest body);

        [Post("/user/add-user-supervisor-for-experience")]
        Task<ApiErrorResult> AddUserSupervisorForExperience([Body] AddUserSupervisorForExperienceRequest body);
    }
}
