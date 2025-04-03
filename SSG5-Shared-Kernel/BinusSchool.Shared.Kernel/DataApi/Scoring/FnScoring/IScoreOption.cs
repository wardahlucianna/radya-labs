using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreOption : IFnScoring
    {
        [Get("/score-settings/get-score-option")]
        Task<ApiErrorResult<IEnumerable<GetScoreOptionResult>>> GetScoreOption(GetScoreOptionRequest query);
    }
}
