using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.SubjectScoreDescription;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface ISubjectScoreDescription : IFnScoring
    {
        [Get("/subjectscoredescription/get-subject-score-description")]
        Task<ApiErrorResult<GetSubjectScoreDescriptionResult>> GetSubjectScoreDescription(GetSubjectScoreDescriptionRequest query);

        [Post("/subjectscoredescription/save-subject-score-description")]
        Task<ApiErrorResult> SaveSubjectScoreDescription(SaveSubjectScoreDescriptionRequest query);

        [Delete("/subjectscoredescription/delete-subject-score-description")]
        Task<ApiErrorResult> DeleteSubjectScoreDescription([Body] DeleteSubjectScoreDescriptionRequest query);
    }
}
