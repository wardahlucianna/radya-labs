using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnBanner.Banner;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas;
using Refit;

namespace BinusSchool.Data.Api.School.FnBanner
{
    public interface IBanner : IFnBanner
    {
        [Get("/banner/ban")]
        Task<ApiErrorResult<IEnumerable<GetBannerResult>>> GetBanners(GetBannerRequest param);

        [Get("/banner/ban/{id}")]
        Task<ApiErrorResult<GetBannerDetailResult>> GetBannerDetail(string id);

        [Post("/banner/ban")]
        Task<ApiErrorResult> AddBanner([Body] AddBannerRequest body);

        [Put("/banner/ban")]
        Task<ApiErrorResult> UpdateBanner([Body] UpdateBannerRequest body);

        [Delete("/banner/ban")]
        Task<ApiErrorResult> DeleteBanner([Body] IEnumerable<string> body);
    }
}
