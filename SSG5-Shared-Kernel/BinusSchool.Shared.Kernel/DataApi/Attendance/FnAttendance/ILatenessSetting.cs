using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Attendance.FnAttendance.LatenessSetting;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface ILatenessSetting : IFnAttendance
    {
        [Get("/lateness-setting/{id}")]
        Task<ApiErrorResult<GetLatenessSettingDetailResult>> GetLatenessSettingDetail(string Id);

        [Post("/lateness-setting")]
        Task<ApiErrorResult> AddAndUpdateLatenessSetting([Body] AddAndUpdateLatenessSettingRequest body);
    }
}
