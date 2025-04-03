using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreComponent;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreComponent : IFnScoring
    {
        [Get("/scorecomponent/master-component")]
        Task<ApiErrorResult<IEnumerable<MasterComponentResult>>> GetMasterScoreComponent(MasterComponentRequest query);

        [Get("/scorecomponent/master-sub-component")]
        Task<ApiErrorResult<IEnumerable<MasterSubComponentResult>>> GetMasterSubScoreComponent(MasterSubComponentRequest query);

        [Post("/scorecomponent/master-sub-component-counter")]
        Task<ApiErrorResult<MasterSubComponentCounterResult>> GetMasterSubScoreComponentCounter([Body] MasterSubComponentCounterRequest query);

    }
}
