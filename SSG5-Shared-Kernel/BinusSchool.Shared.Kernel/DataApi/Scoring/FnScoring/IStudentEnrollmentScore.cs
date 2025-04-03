using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IStudentEnrollmentScore : IFnScoring
    {
        [Get("/studentenrollmentscore/student")]
        Task<ApiErrorResult<IEnumerable<GetStudentEnrollmentScoreResult>>> GetStudentEnrollmentScore(GetStudentEnrollmentScoreRequest query);

        [Get("/student/subject-score-detail")]
        Task<ApiErrorResult<GetStudentSubjectScoreDetailResult>> GetStudentSubjectScoreDetail(GetStudentSubjectScoreDetailRequest query);

        [Get("/student/subject-score-detail-IB")]
        Task<ApiErrorResult<IEnumerable<GetStudentSubjectScoreDetailIBResult>>> GetStudentSubjectScoreDetailIB(GetStudentSubjectScoreDetailIBRequest query);
   
        [Get("/student/subject-score-detail-2")]
        Task<ApiErrorResult<IEnumerable<GetStudentSubjectScoreDetailResult>>> GetStudentSubjectScoreDetail2(GetStudentSubjectScoreDetailRequest query);

        [Get("/student/student-counter-score-forsummary")]
        Task<ApiErrorResult<IEnumerable<GetAllStudentCounterScoreForSummaryResult>>> GetAllStudentCounterScoreForSummary(GetAllStudentCounterScoreForSummaryRequest query);

        [Get("/student/student-counter-score-detail-forsummary")]
        Task<ApiErrorResult<IEnumerable<GetAllStudentCounterScoreForSummaryDetailResult>>> GetAllStudentCounterScoreDetailForSummary(GetAllStudentCounterScoreForSummaryDetailRequest query);

        [Post("/student/export-student-counter-score-forsummary")]
        Task<HttpResponseMessage> ExportStudentCounterScoreForSummary([Body] ExportExcelStudentScoreForSummaryRequest body);
    }
}
