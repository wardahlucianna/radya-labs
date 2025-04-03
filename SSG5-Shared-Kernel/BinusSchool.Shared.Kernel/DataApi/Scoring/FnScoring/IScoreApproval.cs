using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval.ApprovalReportScore;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreApproval : IFnScoring
    {
        [Get("/scoreapproval/getlist-score-approval")]
        Task<ApiErrorResult<GetListScoreApprovalResult>> GetListScoreApproval(GetListScoreApprovalRequest query);
        
        [Get("/scoreapproval/get-history-score-approval")]
        Task<ApiErrorResult<List<GetHistoryScoreApprovalResult>>> GetHistoryScoreApproval(GetHistoryScoreApprovalRequest query);

        [Get("/scoreapproval/get-detail-score-approval")]
        Task<ApiErrorResult<GetDetailScoreApprovalResult>> GetDetailScoreApproval(GetDetailScoreApprovalRequest query);

        [Post("/scoreapproval/save-score-approval-action")]
        Task<ApiErrorResult> SaveScoreApprovalAction(List<SaveScoreApprovalActionRequest> query);

        [Post("/scoreapproval/save-score-approval-action-v2")]
        Task<ApiErrorResult> SaveScoreApprovalActionV2(SaveScoreApprovalActionV2Request query);

        [Get("/scoreapproval/getlist-approval-type")]
        Task<ApiErrorResult<GetListApprovalTypeScoringResult>> GetListApprovalTypeScoring(GetListApprovalTypeScoringRequest query);

        [Get("/scoreapproval/get-approval-report-score")]
        Task<ApiErrorResult<GetApprovalReportScoreResult>> GetApprovalReportScore(GetApprovalReportScoreRequest query);

        [Post("/scoreapproval/save-approval-report-score")]
        Task<ApiErrorResult> SaveApprovalReportScore(SaveApprovalReportScoreRequest query);

        [Post("/scoreapproval/export-excel-score-approval-report")]
        Task<HttpResponseMessage> ExportExcelScoreApprovalReport(ExportExcelScoreApprovalReportRequest query);
    }
}
