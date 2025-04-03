using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.StudentAcademicScore;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IStudentAcademicScore : IFnScoring
    {
        [Get("/academicscoresummary/StudentAcademicScoreForMobile")]
        Task<ApiErrorResult<IEnumerable<GetStudentAcademicScoreForMobileResult>>> StudentAcademicScoreForMobile(GetStudentAcademicScoreForMobileRequest query);

        [Get("/academicscoresummary/SubjectScoreFinalCombined")]
        Task<ApiErrorResult<IEnumerable<GetSubjectScoreFinalCombinedResult>>> SubjectScoreFinalCombined(GetSubjectScoreFinalCombinedRequest query);

        [Get("/academicscoresummary/ScoreSummaryStudentScoreBySubjectType")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummaryStudentScoreBySubjectTypeResult>>> GetScoreSummaryStudentScoreBySubjectType(GetScoreSummaryStudentScoreBySubjectTypeRequest query);
        
        [Get("/academicscoresummary/GetTotalCounterSummaryStudentScoreBySubjectType")]
        Task<ApiErrorResult<GetTotalCounterSummaryStudentScoreBySubjectTypeResult>> GetTotalCounterSummaryStudentScoreBySubjectType(GetTotalCounterSummaryStudentScoreBySubjectTypeRequest query);

        [Get("/academicscoresummary/GetSubjectScoreHistoryBySubjectType")]
        Task<ApiErrorResult<GetSubjectScoreHistoryBySubjectTypeResult>> GetSubjectScoreHistoryBySubjectType(GetSubjectScoreHistoryBySubjectTypeRequest query);
    }
}
