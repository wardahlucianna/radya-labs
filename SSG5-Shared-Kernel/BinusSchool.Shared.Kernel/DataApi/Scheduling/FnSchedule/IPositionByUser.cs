using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IPositionByUser : IFnSchedule
    {
        [Get("/schedule/position-by-user")]
        Task<ApiErrorResult<List<CodeWithIdVm>>> GetPositionByUser(GetPositionByUserRequest param);

        [Post("/schedule/position-by-user/subject")]
        Task<ApiErrorResult<List<GetSubjectByUserResult>>> GetSubjectByUser([Body]GetSubjectByUserRequest body);
    }
}
