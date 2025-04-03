using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingCategory;

namespace BinusSchool.Data.Api.Student.FnGuidanceCounseling
{
    public interface IFnCounselingCategory : IFnGuidanceCounseling
    {
        [Get("/guidance-counseling/counseling-category")]
        Task<ApiErrorResult<IEnumerable<GetCounselingCategoryResult>>> GetCounselingCategory(GetCounselingCategoryRequest query);

        [Get("/guidance-counseling/counseling-category/{id}")]
        Task<ApiErrorResult<GetDetailCounselingCategoryResult>> GetDetailCounselingCategory(string id);

        [Post("/guidance-counseling/counseling-category")]
        Task<ApiErrorResult> AddCounselingCategory([Body] AddCounselingCategoryRequest body);

        [Put("/guidance-counseling/counseling-category")]
        Task<ApiErrorResult> UpdateCounselingCategory([Body] UpdateCounselingCategoryRequest body);

        [Delete("/guidance-counseling/counseling-category")]
        Task<ApiErrorResult> DeleteCounselingCategory([Body] IEnumerable<string> ids);
    }
}
