using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface ISession : IFnSchedule
    {
        [Get("/schedule/session")]
        Task<ApiErrorResult<IEnumerable<GetPathwayResult>>> GetSessions(GetSessionRequest query);

        [Get("/schedule/session/{id}")]
        Task<ApiErrorResult<GetPathwayResult>> GetSessionDetail(string id);

        [Get("/schedule/session-by-sessionset")]
        Task<ApiErrorResult<List<GetSessionAscTimetableResult>>> GetSessionListForHelperAsc([Query] GetSessionBySessionSetRequest query);

        [Post("/schedule/session")]
        Task<ApiErrorResult> AddSession([Body] AddSessionRequest body);

        [Post("/schedule/session/add-from-asc")]
        Task<ApiErrorResult> AddSessionFromAsc([Body] List<AddSessionFromAscRequest> body);

        [Put("/schedule/session")]
        Task<ApiErrorResult> UpdateSession([Body] UpdateSessionRequest body);

        [Delete("/schedule/session")]
        Task<ApiErrorResult> DeleteSession([Body] IEnumerable<string> ids);

        [Post("/schedule/session/copy")]
        Task<ApiErrorResult> CopySession([Body] CopySessionRequest body);

        [Get("/schedule/session/day")]
        Task<ApiErrorResult<IEnumerable<GetSessionDayResult>>> GetSessionDay(GetSessionDayRequest query);
    }
}
