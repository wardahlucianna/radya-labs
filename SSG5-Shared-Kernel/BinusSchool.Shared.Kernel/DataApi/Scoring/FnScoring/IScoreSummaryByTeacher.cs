using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByTeacher;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreSummaryByTeacher : IFnScoring
    {
        [Post("/scoresummary/get-teacher-list")]
        Task<ApiErrorResult<IEnumerable<GetTeacherListResult>>> GetTeacherListResult([Body] GetTeacherListRequest body);

        [Get("/scoresummary/summary-byteacher")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummaryByTeacherResult>>> GetScoreSummaryByTeacher(GetScoreSummaryByTeacherRequest query);

        [Post("/scoresummary/summary-byteacher-excel")]
        Task<HttpResponseMessage> ExportExcelScoreSummaryByTeacher([Body] ExportExcelScoreSummaryByTeacherRequest body);

        [Get("/scoresummary/summary-detail-byteacher")]
        Task<ApiErrorResult<IEnumerable<GetDetailScoreSummaryByTeacherResult>>> GetDetailScoreSummaryByTeacher(GetDetailScoreSummaryByTeacherRequest query);

        [Post("/scoresummary/summary-detail-byteacher-excel")]
        Task<HttpResponseMessage> ExportExcelDetailScoreSummaryByTeacher([Body] ExportExcelDetailScoreSummaryByTeacherRequest body);
    }
}
