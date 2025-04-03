using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IRolePosition : IFnSchedule
    {
        [Get("/schedule/role-position")]
        Task<ApiErrorResult<List<CodeWithIdVm>>> GetPositionByUser(GetPositionByUserRequest param);

        [Post("/schedule/role-position/subject")]
        Task<ApiErrorResult<List<GetSubjectByUserResult>>> GetSubjectByUser([Body]GetSubjectByUserRequest body);

        [Post("/schedule/role-position/user")]
        Task<ApiErrorResult<List<GetUserRolePositionResult>>> GetUserRolePosition([Body]GetUserRolePositionRequest body);

        [Post("/schedule/role-position/user-email-recepient")]
        Task<ApiErrorResult<List<GetUserSubjectByEmailRecepientResult>>> GetUserSubjectByEmailRecepient([Body] GetUserSubjectByEmailRecepientRequest body);
    }
}
