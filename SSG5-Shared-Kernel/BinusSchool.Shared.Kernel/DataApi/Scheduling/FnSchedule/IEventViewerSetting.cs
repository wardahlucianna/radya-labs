using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EventViewerSetting;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IEventViewerSetting : IFnSchedule
    {
        [Get("/schedule/EventViewerSetting")]
        Task<ApiErrorResult<IEnumerable<EventViewerSettingResult>>> GetEventViewerSettings(GetEventViewerSettingRequest query);

        [Get("/schedule/EventViewerSetting/{id}")]
        Task<ApiErrorResult<EventViewerSettingResult>> GetEventViewerSettingDetail(string id);

        [Post("/schedule/EventViewerSetting")]
        Task<ApiErrorResult> AddActivityRequest([Body] AddEventViewerSettingRequest body);

        [Put("/schedule/EventViewerSetting")]
        Task<ApiErrorResult> UpdateEventViewerSetting([Body] UpdateEventViewerSettingRequest body);

        [Delete("/schedule/EventViewerSetting")]
        Task<ApiErrorResult> DeleteEventViewerSetting([Body] IEnumerable<string> ids);
    }
}
