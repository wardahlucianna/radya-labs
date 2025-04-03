using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreAdditionalCode;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreAdditionalCode : IFnScoring
    {
        [Get("/scoring/score-additional-code")]
        Task<ApiErrorResult<IEnumerable<GetScoreAdditionalCodeResult>>> GetScoreAdditionalCode(GetScoreAdditionalCodeRequest query);
    }
}
