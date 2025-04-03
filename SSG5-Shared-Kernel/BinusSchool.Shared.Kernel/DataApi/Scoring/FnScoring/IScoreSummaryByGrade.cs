using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByGrade;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreSummaryByGrade : IFnScoring
    {
        [Get("/scoresummary/summary-byGrade")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummaryByGradeResult>>> GetScoreSummaryByGrade(GetScoreSummaryByGradeRequest query);

        [Post("/scoresummary/summary-byGrade-excel")]
        Task<HttpResponseMessage> ExportExcelScoreSummaryByGrade([Body] ExportExcelScoreSummaryByGradeRequest body);
    }
}
