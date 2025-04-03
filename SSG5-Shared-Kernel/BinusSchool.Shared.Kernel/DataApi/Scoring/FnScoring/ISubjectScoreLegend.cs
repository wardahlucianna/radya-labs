using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.SubjectScoreLegend;
using Refit;


namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface ISubjectScoreLegend : IFnScoring
    {
        [Get("/scoring/subject-score-legend")]
        Task<ApiErrorResult<IEnumerable<GetSubjectScoreLegendResult>>> GetSubjectScoreLegend(GetSubjectScoreLegendRequest query);
    }
}
