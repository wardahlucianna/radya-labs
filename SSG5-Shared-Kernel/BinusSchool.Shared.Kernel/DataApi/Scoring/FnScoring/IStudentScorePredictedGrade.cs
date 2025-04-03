using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.StudentScorePredictedGrade;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IStudentScorePredictedGrade : IFnScoring
    {
        [Get("/studentscore-predictedgrade/get")]
        Task<ApiErrorResult<GetStudentScorePredictedGradeResult>> GetStudentScorePredictedGrade(GetStudentScorePredictedGradeRequest query);

        [Put("/studentscore-predictedgrade/update-score")]
        Task<ApiErrorResult> UpdateStudentScorePredictedGrade([Body] UpdateStudentScorePredictedGradeRequest body);
    }
}
