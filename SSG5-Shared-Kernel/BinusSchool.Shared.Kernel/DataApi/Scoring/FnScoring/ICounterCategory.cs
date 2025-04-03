using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.CounterCategory;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{

    public interface ICounterCategory : IFnScoring
    {
        [Get("/scoring/counter-category")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetCounterCategory(GetCounterCategoryRequest req);

    }
}
