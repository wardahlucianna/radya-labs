using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SessionSet;
using BinusSchool.Data.Model.School.FnPeriod.SessionSet;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface ISessionSet : IFnSchedule
    {
        [Get("/schedule/session-set")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetSessionSets(CollectionSchoolRequest query);

        [Get("/schedule/session-set/with-level-grade")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetSessionSetWithLevelGrade(GetByLevelGradeRequest query);

        [Post("/schedule/session-set")]
        Task<ApiErrorResult<string>> AddSessionSet([Body] AddSessionSetRequest body);

        [Put("/schedule/session-set")]
        Task<ApiErrorResult> UpdateSessionSet([Body] UpdateSessionSetRequest body);

        [Delete("/schedule/session-set")]
        Task<ApiErrorResult> DeleteSessionSet([Body] IEnumerable<string> ids);
        
        [Delete("/schedule/session-set/delete-session-set-from-asc")]
        Task<ApiErrorResult> DeleteSessionSetFromAsc(DeleteSessionSetFromAscRequest query);
    }
}
