using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AvailabilitySetting;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IAvailabilitySetting : IFnSchedule
    {
        [Get("/schedule/availability-setting")]
        Task<ApiErrorResult<IEnumerable<GetAvailabilitySettingResult>>> GetAvailabilitySetting(GetAvailabilitySettingRequest query);

        [Get("/schedule/availability-setting/Detail")]
        Task<ApiErrorResult<DetailAvailabilitySettingResult>> DetailAvailabilitySetting(DetailAvailabilitySettingRequest query);

        [Post("/schedule/availability-setting")]
        Task<ApiErrorResult> AddAvailabilitySetting(AddAvailabilitySettingRequest query);


    }
}
