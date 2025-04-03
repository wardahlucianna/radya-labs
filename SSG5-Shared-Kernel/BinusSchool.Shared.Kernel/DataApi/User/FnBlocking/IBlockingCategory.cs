using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.BlockingCategory;
using Refit;

namespace BinusSchool.Data.Api.User.FnBlocking.UserBlocking
{
    public interface IBlockingCategory : IFnBlocking
    {
        [Get("/user-blocking/blocking-category")]
        Task<ApiErrorResult<IEnumerable<GetBlockingCategoryResult>>> GetBlockingCategory(GetBlockingCategoryRequest query);

        [Get("/user-blocking/blocking-category/detail/{id}")]
        Task<ApiErrorResult<GetBlockingCategoryDetailResult>> GetBlockingCategoryDetail(string id);

        [Post("/user-blocking/blocking-category")]
        Task<ApiErrorResult> AddBlockingCategory([Body] AddBlockingCategoryRequest body);

        [Put("/user-blocking/blocking-category")]
        Task<ApiErrorResult> UpdateBlockingCategory([Body] UpdateBlockingCategoryRequest body);

        [Delete("/user-blocking/blocking-category")]
        Task<ApiErrorResult> DeleteBlockingCategory([Body] IEnumerable<string> ids);

        [Get("/user-blocking/blocking-category/by-user")]
        Task<ApiErrorResult<IEnumerable<GetBlockingByUserResult>>> GetBlockingCategoryByUser(GetBlockingByUserRequest query);
    }
}
