using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.MasterWorkhabit;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IMasterWorkhabit : IFnAttendance
    {
        [Get("/attendance/workhabit")]
        Task<ApiErrorResult<IEnumerable<GetWorkhabitResult>>> GetWorkhabits(GetWorkhabitRequest request);

        [Get("/attendance/workhabit/{id}")]
        Task<ApiErrorResult<GetWorkhabitResult>> GetWorkhabitDetail(string id);

        [Post("/attendance/workhabit")]
        Task<ApiErrorResult> AddWorkhabit([Body] AddWorkhabitRequest body);

        [Put("/attendance/workhabit")]
        Task<ApiErrorResult> UpdateWorkhabit([Body] UpdateWorkhabitRequest body);

        [Delete("/attendance/workhabit")]
        Task<ApiErrorResult> DeleteWorkhabit([Body] IEnumerable<string> ids);
    }
}
