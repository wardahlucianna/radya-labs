using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ConcernCategory;

namespace BinusSchool.Data.Api.Student.FnGuidanceCounseling
{
    public interface IFnConcernCategory : IFnGuidanceCounseling
    {
        [Get("/guidance-counseling/concern-category")]
        Task<ApiErrorResult<IEnumerable<GetConcernCategoryResult>>> GetConcernCategory(GetConcernCategoryRequest query);

        [Get("/guidance-counseling/concern-category/{id}")]
        Task<ApiErrorResult<GetDetailConcernCategoryResult>> GetDetailConcernCategory(string id);

        [Post("/guidance-counseling/concern-category")]
        Task<ApiErrorResult> AddConcernCategory([Body] AddConcernCategoryRequest body);

        [Put("/guidance-counseling/concern-category")]
        Task<ApiErrorResult> UpdateConcernCategory([Body] UpdateConcernCategoryRequest body);

        [Delete("/guidance-counseling/concern-category")]
        Task<ApiErrorResult> DeleteConcernCategory([Body] IEnumerable<string> ids);
    }
}
