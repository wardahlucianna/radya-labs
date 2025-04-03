using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByHomeroom;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreSummaryByHomeroom : IFnScoring
    {
        [Get("/scoresummary/summary-byHomeroom")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummaryByHomeroomResult>>> GetScoreSummaryByHomeroom(GetScoreSummaryByHomeroomRequest query);

        [Get("/scoresummary/summary-statistic-header-byHomeroom")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummaryStatisticHeaderByHomeroomResult>>> GetScoreSummaryStatisticHeaderByHomeroom(GetScoreSummaryStatisticHeaderByHomeroomRequest query);

        [Get("/scoresummary/summary-statistic-category-byHomeroom")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummaryStatisticCategoryByHomeroomResult>>> GetScoreSummaryStatisticCategoryByHomeroom(GetScoreSummaryStatisticCategoryByHomeroomRequest query);

        [Get("/scoresummary/summary-statistic-byHomeroom")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummaryStatisticByHomeroomResult>>> GetScoreSummaryStatisticByHomeroom(GetScoreSummaryStatisticByHomeroomRequest query);

        [Get("/scoresummary/summary-term-report-byhomeroom")]
        Task<ApiErrorResult<GetScoreSummaryTermReportByHomeroomResult>> GetScoreSummaryTermReportByHomeroom(GetScoreSummaryTermReportByHomeroomRequest query);

        [Post("/scoresummary/summary-byHomeroom-excel")]
        Task<HttpResponseMessage> ExportExcelScoreSummaryByHomeroom([Body] ExportExcelScoreSummaryByHomeroomRequest body);

        [Post("/scoresummary/export-summary-term-report-byHomeroom-excel")]
        Task<HttpResponseMessage> ExportExcelScoreSummaryTermReportByHomeroom([Body] ExportExcelScoreSummaryTermReportByHomeroomRequest body);

        [Get("/scoresummary/summary-teacher-comment-byhomeroom")]
        Task<ApiErrorResult<GetScoreSummaryTeacherCommentByHomeroomResult>> GetScoreSummaryTeacherCommentByHomeroom(GetScoreSummaryTeacherCommentByHomeroomRequest query);

        [Post("/scoresummary/export-summary-teacher-comment-byHomeroom-excel")]
        Task<HttpResponseMessage> ExportExcelScoreSummaryTeacherCommentByHomeroom([Body] ExportExcelScoreSummaryTeacherCommentByHomeroomRequest body);
    }
}
