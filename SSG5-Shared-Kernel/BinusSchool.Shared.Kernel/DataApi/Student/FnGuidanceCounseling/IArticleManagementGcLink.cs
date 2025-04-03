using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementGcLink;
using Refit;
namespace BinusSchool.Data.Api.Student.FnGuidanceCounseling
{
    public interface IArticleManagementGcLink : IFnGuidanceCounseling
    {
        [Get("/guidance-counseling/article-management-gc-link")]
        Task<ApiErrorResult<IEnumerable<GetArticleManagementGcLinkResult>>> GetListArticleManagementGcLink(GetArticleManagementGcLinkRequest query);

        [Get("/guidance-counseling/article-management-gc-link/detail/{id}")]
        Task<ApiErrorResult<GetArticleManagementGcLinkResult>> GetArticleManagementGcLinkDetail(string id);

        [Post("/guidance-counseling/article-management-gc-link")]
        Task<ApiErrorResult> AddArticleManagementGcLink([Body] AddArticleManagementGcLinkRequest body);

        [Put("/guidance-counseling/article-management-gc-link")]
        Task<ApiErrorResult> UpdateArticleManagementGcLink([Body] UpdateArticleManagementGcLinkRequest body);

        [Delete("/guidance-counseling/article-management-gc-link")]
        Task<ApiErrorResult> DeleteArticleManagementGcLink([Body] IEnumerable<string> ids);

    }
}
