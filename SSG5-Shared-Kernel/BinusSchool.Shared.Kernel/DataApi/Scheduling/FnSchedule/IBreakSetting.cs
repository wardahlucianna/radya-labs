using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.BreakSetting;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IBreakSetting : IFnSchedule
    {

        [Get("/schedule/break-setting")]
        Task<ApiErrorResult<IEnumerable<GetBreakSettingResult>>> GetBreakSetting(GetBreakSettingRequest query);

        [Get("/schedule/break-setting-v2")]
        Task<ApiErrorResult<IEnumerable<GetBreakSettingResult>>> GetBreakSettingV2(GetBreakSettingRequest query);

        [Post("/schedule/break-setting-priority-flexible-break")]
        Task<ApiErrorResult> UpdatePriorityAndFlexibleBreak([Body]UpdatePriorityAndFlexibleBreakRequest body);

        [Post("/schedule/break-setting-availability")]
        Task<ApiErrorResult> UpdateAvailability([Body]UpdateAvailabilityRequest body);
    }
}
