using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceBlockingSetting;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IAttendanceBlockingSetting : IFnAttendance
    {
        [Get("/attendance-blocking-setting/mapping")]
        Task<ApiErrorResult<IEnumerable<GetBlockingMapAttendanceAYResult>>> GetBlockingMapAttendanceAY(GetBlockingMapAttendanceAYRequest param);

        [Get("/attendance-blocking-setting/detail")]
        Task<ApiErrorResult<IEnumerable<GetAttendanceBlockingSettingDetailResult>>> GetAttendanceBlockingSetting(GetAttendanceBlockingSettingDetailRequest param);

        [Put("/attendance-blocking-setting")]
        Task<ApiErrorResult> UpdateAttendanceBlockingSetting([Body] UpdateAttendanceBlockingSettingRequest body);

        [Get("/attendance-blocking-setting/message")]
        Task<ApiErrorResult<GetAttendanceBlockingSettingMessageResult>> GetAttendanceBlockingSettingMessage(GetAttendanceBlockingSettingMessageRequest query);
    }
}
