using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ShowScoreSummaryTab;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IShowScoreSummaryTab : IFnScoring
    {
        [Get("/scoresummary/tab")]
        Task<ApiErrorResult<GetShowScoreSummaryTabResult>> GetShowScoreSummaryTab(GetShowScoreSummaryTabRequest query);
    }
}
