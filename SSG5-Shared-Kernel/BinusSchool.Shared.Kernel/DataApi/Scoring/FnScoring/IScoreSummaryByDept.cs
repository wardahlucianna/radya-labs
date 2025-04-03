using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByDept;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreSummaryByDept : IFnScoring
    {
        [Get("/scoresummary/summary-bydepartment")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummaryByDeptResult>>> GetScoreSummaryByDept(GetScoreSummaryByDeptRequest query);

        [Post("/scoresummary/summary-bydepartment-excel")]
        Task<HttpResponseMessage> ExportExcelScoreSummaryByDept([Body] ExportExcelScoreSummaryByDeptRequest body);

        [Get("/scoresummary/summary-detail-bydepartment")]
        Task<ApiErrorResult<IEnumerable<GetDetailScoreSummaryByDeptResult>>> GetDetailScoreSummaryByDept(GetDetailScoreSummaryByDeptRequest query);

        [Post("/scoresummary/summary-detail-bydepartment-excel")]
        Task<HttpResponseMessage> ExportExcelDetailScoreSummaryByDept([Body] ExportExcelDetailScoreSummaryByDeptRequest body);
    }
}
