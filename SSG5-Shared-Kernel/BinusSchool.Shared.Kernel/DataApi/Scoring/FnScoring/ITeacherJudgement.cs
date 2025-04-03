using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.TeacherJudgement;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface ITeacherJudgement : IFnScoring
    {
        [Get("/scoring/teacher-judgement-option")]
        Task<ApiErrorResult<IEnumerable<GetTeacherJudgementOptionsResult>>> GetTeacherJudgementOptions(GetTeacherJudgementOptionsRequest query);

        [Get("/scoring/student-teacher-judgement")]
        Task<ApiErrorResult<GetStudentTeacherJudgementResult>> GetStudentTeacherJudgement(GetStudentTeacherJudgementRequest query);

        [Put("/scoring/teacher-judgement-update")]
        Task<ApiErrorResult> UpdateScoreFromTeacherJudgement([Body] UpdateStudentTeacherJudgementRequest body);
    }
}
