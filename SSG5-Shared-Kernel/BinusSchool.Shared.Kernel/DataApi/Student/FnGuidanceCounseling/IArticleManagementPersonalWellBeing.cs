using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementPersonalWellBeing;
using Refit;

namespace BinusSchool.Data.Api.Student.FnGuidanceCounseling
{
    public interface IArticleManagementPersonalWellBeing : IFnGuidanceCounseling
    {
        [Get("/guidance-counseling/article-management-personal-well-being")]
        Task<ApiErrorResult<IEnumerable<GetArticleManagementPersonalWellBeingResult>>> GetListArticleManagementPersonalWellBeing(GetArticleManagementPersonalWellBeingRequest query);
              
        [Get("/guidance-counseling/article-management-personal-well-being/detail/{id}")]
        Task<ApiErrorResult<GetArticleManagementPersonalWellBeingResult>> GetArticleManagementPersonalWellBeingDetail(string id);

        [Post("/guidance-counseling/article-management-personal-well-being")]
        Task<ApiErrorResult> AddArticleManagementPersonalWellBeing([Body] AddArticleManagementPersonalWellBeingRequest body);

        [Put("/guidance-counseling/article-management-personal-well-being")]
        Task<ApiErrorResult> UpdateArticleManagementPersonalWellBeing([Body] UpdateArticleManagementPersonalWellBeingRequest body);

        [Delete("/guidance-counseling/article-management-personal-well-being")]
        Task<ApiErrorResult> DeleteArticleManagementPersonalWellBeing([Body] IEnumerable<string> ids);

    }
}
