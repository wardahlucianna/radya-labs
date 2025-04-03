using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface ISchedule : IFnSchedule
    {
        [Get("/schedule")]
        Task<ApiErrorResult<IEnumerable<ScheduleVm>>> GetSchedules(GetScheduleRequest query);

        [Get("/schedule/detail/{id}")]
        Task<ApiErrorResult<ScheduleDetailResult>> GetScheduleDetail(string id);

        [Post("/schedule/add")]
        Task<ApiErrorResult> AddSchedule([Body] AddScheduleRequest body);

        [Put("/schedule/update")]
        Task<ApiErrorResult> UpdateSchedule([Body] UpdateScheduleRequest body);

        [Delete("/schedule/delete")]
        Task<ApiErrorResult> DeleteSchedule(string IdSchedule);

        [Get("/schedule/venue")]
        Task<ApiErrorResult<bool>> CheckScheduleByVenue(GetScheduleByVenueRequest query);
    }
}

