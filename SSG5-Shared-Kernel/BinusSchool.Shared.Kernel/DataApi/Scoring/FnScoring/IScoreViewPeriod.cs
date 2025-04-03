using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreViewPeriod;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreViewPeriod : IFnScoring
    {
        [Get("/scoreviewperiod/view-block")]
        Task<ApiErrorResult<GetScoreViewPeriodForBlockResult>> GetScoreViewPeriodForBlock(GetScoreViewPeriodForBlockRequest query);
    }
}
