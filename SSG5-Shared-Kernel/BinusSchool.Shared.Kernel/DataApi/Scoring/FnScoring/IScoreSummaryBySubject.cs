using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryBySubject;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreSummaryBySubject : IFnScoring
    {
        [Get("/scoresummary/summary-subject-option")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummarySubjectOptionResult>>> GetScoreSummarySubjectOptions(GetScoreSummarySubjectOptionRequest query);

        [Get("/scoresummary/summary-bySubject")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummaryBySubjectResult>>> GetScoreSummaryBySubject(GetScoreSummaryBySubjectRequest query);

        [Get("/scoresummary/summary-statistic-header-bySubject")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummaryStatisticHeaderBySubjectResult>>> GetScoreSummaryStatisticHeaderBySubject(GetScoreSummaryStatisticHeaderBySubjectRequest query);

        [Get("/scoresummary/summary-statistic-bySubject")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummaryStatisticBySubjectResult>>> GetScoreSummaryStatisticBySubject(GetScoreSummaryStatisticBySubjectRequest query);

        [Post("/scoresummary/summary-bySubject-excel")]
        Task<HttpResponseMessage> ExportExcelScoreSummaryBySubject([Body] ExportExcelScoreSummaryBySubjectRequest body);
    }
}
