using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.EntryScore;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IEntryScore  : IFnScoring
    {
        [Post("/entryscore/component-score")]
        Task<ApiErrorResult<IEnumerable<GetComponentEntryScoreResult>>> GetComponentEntryScore([Body] GetComponentEntryScoreRequest query);

        [Post("/entryscore/component-approval-report")]
        Task<ApiErrorResult<IEnumerable<GetComponentApprovalReportResult>>> GetComponentApprovalReport(GetComponentApprovalReportRequest query);

        [Get("/entryscore/setting-entry-score")]
        Task<ApiErrorResult<IEnumerable<GetSettingEntryScoreResult>>> GetSettingEntryScore(GetSettingEntryScoreRequest query);

        [Post("/entryscore/student-score")]
        Task<ApiErrorResult<IEnumerable<GetStudentScoreEntryResult>>> GetStudentScoreEntry([Body] GetStudentScoreEntryRequest query);

        [Put("/entryscore/update-entry-score")]
        Task<ApiErrorResult> UpdateEntryScore([Body] UpdateEntryScoreRequest query);

        [Post("/entryscore/student-score-detail-bystatus")]
        Task<ApiErrorResult<GetStudentScoreDetailByStatusResult>> GetStudentScoreDetailByStatus([Body] GetStudentScoreDetailByStatusRequest query);

        [Post("/entryscore/exportexcel-student-score-detail-bystatus")]
        Task<HttpResponseMessage> ExportExcelStudentScoreDetailByStatus([Body] GetStudentScoreDetailByStatusRequest body);

        [Delete("/entryscore/delete-entry-counter-score")]
        Task<ApiErrorResult> DeleteStudentCounterScore([Body] DeleteStudentCounterScoreRequest query);

        [Post("/entryscore/update-assessment-score")]
        Task<ApiErrorResult> UpdateAssessmentScore([Body] UpdateAssessmentScoreRequest query);

        [Put("/entryscore/send-email-entry-score")]
        Task<ApiErrorResult> SendEmailNotificationEntryScore([Body] SendEmailNotificationEntryScoreRequest query);
    }
}
