using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Award;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IAward : IFnSchedule
    {
        [Get("/schedule/award")]
        Task<ApiErrorResult<IEnumerable<AwardResult>>> GetAwards(GetAwardRequest query);

        [Get("/schedule/award/{id}")]
        Task<ApiErrorResult<AwardResult>> GetAwardDetail(string id);

        [Post("/schedule/award")]
        Task<ApiErrorResult> AddAward([Body] CreateAwardRequest body);

        [Put("/schedule/award")]
        Task<ApiErrorResult> UpdateAward([Body] UpdateAwardRequest body);

        [Delete("/schedule/award")]
        Task<ApiErrorResult> DeleteAward([Body] IEnumerable<string> ids);

        [Put("/schedule/award/set-recommendation")]
        Task<ApiErrorResult> SetAwardRecommendationStatus([Body] SetAwardRecommendationStatusRequest body);
    }
}
