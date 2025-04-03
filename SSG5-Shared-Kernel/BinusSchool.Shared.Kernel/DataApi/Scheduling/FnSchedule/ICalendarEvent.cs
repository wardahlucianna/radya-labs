using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface ICalendarEvent : IFnSchedule
    {
        [Get("/schedule/calendar/event-v2")]
        Task<ApiErrorResult<IEnumerable<GetCalendarEvent2Result>>> GetEvents2(GetCalendarEvent2Request query);
        
        [Get("/schedule/calendar/event-v2/detail/{id}")]
        Task<ApiErrorResult<GetCalendarEvent2DetailResult>> GetEventDetail2(string id);

        [Post("/schedule/calendar/event-v2")]
        Task<ApiErrorResult> AddEvent2([Body] AddCalendarEvent2Request body);

        [Put("/schedule/calendar/event-v2")]
        Task<ApiErrorResult> UpdateEvent2([Body] UpdateCalendarEvent2Request body);

        [Delete("/schedule/calendar/event-v2")]
        Task<ApiErrorResult> DeleteEvent2([Body] IEnumerable<string> ids);
    }
}
