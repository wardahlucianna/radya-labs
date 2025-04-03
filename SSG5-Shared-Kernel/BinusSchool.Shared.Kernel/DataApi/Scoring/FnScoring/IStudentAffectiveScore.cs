using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.StudentAffectiveScore;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IStudentAffectiveScore : IFnScoring
    {
        [Get("/affectivescoresummary/view-detail")]
        Task<ApiErrorResult<GetScoreViewStudentAffectiveScoreDetailResult>> GetScoreViewStudentAffectiveScoreDetail(GetScoreViewStudentAffectiveScoreDetailRequest query);

        [Get("/affectivescoresummary/student-subject-detail")]
        Task<ApiErrorResult<GetStudentSubjectAffectiveScoreDetailResult>> GetStudentSubjectAffectiveScoreDetail(GetStudentSubjectAffectiveScoreDetailRequest query);
    }
}
