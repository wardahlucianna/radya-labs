using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.SubjectType;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoringSubjectType : IFnScoring
    {
        [Get("/score/subject-type")]
        Task<ApiErrorResult<GetScoringSubjectTypeResult>> GetScoringSubjectType(GetScoringSubjectTypeRequest query);
    }
}
