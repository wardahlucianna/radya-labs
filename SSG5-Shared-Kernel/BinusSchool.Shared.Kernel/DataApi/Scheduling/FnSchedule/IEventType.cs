using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EventType;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IEventType : IFnSchedule
    {
        [Get("/schedule/event-type")]
        Task<ApiErrorResult<IEnumerable<EventTypeResult>>> GetEventTypes(GetEventTypeRequest query);

        [Get("/schedule/event-type/{id}")]
        Task<ApiErrorResult<EventTypeResult>> GetEventTypeDetail(string id);

        [Post("/schedule/event-type")]
        Task<ApiErrorResult> AddEventType([Body] AddEventTypeRequest body);

        [Put("/schedule/event-type")]
        Task<ApiErrorResult> UpdateEventType([Body] UpdateEventTypeRequest body);

        [Delete("/schedule/event-type")]
        Task<ApiErrorResult> DeleteEventType([Body] IEnumerable<string> ids);

        [Get("/schedule/get-event-by-eventtype")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetEventbyEventType(GetEventbyEventTypeRequest query);
    }
}
