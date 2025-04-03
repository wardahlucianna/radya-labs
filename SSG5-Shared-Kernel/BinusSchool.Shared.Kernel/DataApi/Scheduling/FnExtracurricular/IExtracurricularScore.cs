using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IExtracurricularScore : IFnExtracurricular
    {
        [Get("/extracurricular/score/unsubmitted")]
        Task<ApiErrorResult<IEnumerable<GetUnSubmittedScoreResult>>> GetUnSubmittedScore(GetUnSubmittedScoreRequest body);

        [Get("/extracurricular/score/extracurricular-score-entry")]
        Task<ApiErrorResult<GetExtracurricularResult>> GetExtracurricularScoreEntry(GetExtracurricularRequest body);

        [Get("/extracurricular/score/student-score")]
        Task<ApiErrorResult<GetExtracurricularStudentScoreResult>> GetExtracurricularStudentScore(GetExtracurricularStudentScoreRequest body);

        [Get("/extracurricular/score/get-score-legend")]
        Task<ApiErrorResult<IEnumerable<GetExtracurricularScoreLegendResult>>> GetExtracurricularScoreLegend(GetExtracurricularScoreLegendRequest body);
              
        [Put("/extracurricular/score/update-student-score")]
        Task<ApiErrorResult> UpdateExtracurricularStudentScore([Body] UpdateExtracurricularStudentScoreRequest body);

        [Put("/extracurricular/score/update-score-legend")]
        Task<ApiErrorResult> UpdateExtracurricularScoreLegend([Body] UpdateExtracurricularScoreLegendRequest body);

        [Post("/extracurricular/score/export-excel-student-score")]
        Task<HttpResponseMessage> ExportExcelExtracurricularStudentScore(GetExtracurricularStudentScoreRequest body);

        [Put("/extracurricular/score/update-score-legend-v2")]
        Task<ApiErrorResult> UpdateExtracurricularScoreLegend2([Body] UpdateExtracurricularScoreLegendRequest2 body);

        [Get("/extracurricular/score/get-score-legend-v2")]
        Task<ApiErrorResult<IEnumerable<GetExtracurricularScoreLegendResult2>>> GetExtracurricularScoreLegend2(GetExtracurricularScoreLegendRequest2 body);

        [Get("/extracurricular/score/extracurricular-score-entry-v2")]
        Task<ApiErrorResult<IEnumerable<GetExtracurricularResult2>>> GetExtracurricularScoreEntry2(GetExtracurricularRequest2 body);

        [Post("/extracurricular/score/export-excel-unsubmiited-score")]
        Task<HttpResponseMessage> ExportExcelUnSubmittedScore(GetUnSubmittedScoreRequest body);

    }
}
