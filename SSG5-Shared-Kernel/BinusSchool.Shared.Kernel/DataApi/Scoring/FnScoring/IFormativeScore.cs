using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.EntryScore;
using BinusSchool.Data.Model.Scoring.FnScoring.FormativeScoreEntry;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IFormativeScore : IFnScoring
    {
        [Post("/entryscore/FormativeScoreEntry")]
        Task<ApiErrorResult<IEnumerable<GetStudentScoreEntryResult>>> GetFormativeScoreEntry([Body] GetFormativeScoreEntryRequest query);

        [Put("/entryscore/AddFormativeScoreEntry")]
        Task<ApiErrorResult> AddFormativeScoreEntry([Body] UpdateEntryScoreRequest query);

    }
}
