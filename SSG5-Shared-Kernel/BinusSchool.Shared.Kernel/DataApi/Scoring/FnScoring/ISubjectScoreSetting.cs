using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.SubjectScoreSetting;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface ISubjectScoreSetting : IFnScoring
    {
        [Get("scoring/subject-score-setting")]
        Task<ApiErrorResult<IEnumerable<GetSubjectScoreSettingResult>>> GetClassPrivilegeforEntryScore(GetSubjectScoreSettingRequest query);

    }
}
