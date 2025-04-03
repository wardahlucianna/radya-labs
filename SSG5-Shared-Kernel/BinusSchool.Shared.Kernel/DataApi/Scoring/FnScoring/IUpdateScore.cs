using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.UpdateScore;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IUpdateScore : IFnScoring
    {
        [Get("/updatescore/student-score-forapproval")]
        Task<ApiErrorResult<GetStudentScoreForApprovalResult>> GetStudentScoreForApproval(GetStudentScoreForApprovalRequest query);

        [Put("/updatescore/update-score")]
        Task<ApiErrorResult> UpdateScore([Body] UpdateScoreRequest query);

        [Put("/updatescore/approval-update-score")]
        Task<ApiErrorResult> ApprovalUpdateScore([Body] ApprovalUpdateScoreRequest query);

        [Put("/updatescore/rekalkulasi-student-score")]
        Task<ApiErrorResult> UpdateRekalkulasiStudentScore([Body] UpdateRekalkulasiStudentScoreRequest query);

        [Put("/updatescore/rekalkulasi-score-by-subject-mapping")]
        Task<ApiErrorResult> UpdateRekalkulasiScoreBySubjectMapping([Body] UpdateRekalkulasiScoreBySubjectMappingRequest query);

    }
}
