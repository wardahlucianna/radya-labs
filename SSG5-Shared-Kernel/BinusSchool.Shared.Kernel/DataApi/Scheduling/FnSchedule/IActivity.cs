using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Activity;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IActivity : IFnSchedule
    {
        [Get("/schedule/activity")]
        Task<ApiErrorResult<IEnumerable<ActivityResult>>> GetActivitys(GetActivityRequest query);

        [Get("/schedule/activity/{id}")]
        Task<ApiErrorResult<ActivityResult>> GetActivityDetail(string id);

        [Post("/schedule/activity")]
        Task<ApiErrorResult> AddActivity([Body] AddActivityRequest body);

        [Put("/schedule/activity")]
        Task<ApiErrorResult> UpdateActivity([Body] UpdateActivityRequest body);

        [Delete("/schedule/activity")]
        Task<ApiErrorResult> DeleteActivity([Body] IEnumerable<string> ids);
    }
}
