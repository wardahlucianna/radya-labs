using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByLevel;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreSummaryByLevel : IFnScoring
    {
        [Get("/scoresummary/summary-byLevel")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummaryByLevelResult>>> GetScoreSummaryByLevel(GetScoreSummaryByLevelRequest query);
        
        [Post("/scoresummary/summary-byLevel-excel")]
        Task<HttpResponseMessage> ExportExcelScoreSummaryByLevel([Body] ExportExcelScoreSummaryByLevelRequest body);
    }
}
