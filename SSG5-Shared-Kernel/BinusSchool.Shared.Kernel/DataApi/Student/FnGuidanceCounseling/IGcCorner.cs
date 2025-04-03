using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner;
using Refit;

namespace BinusSchool.Data.Api.Student.FnGuidanceCounseling
{
    public interface IGcCorner : IFnGuidanceCounseling
    {
        [Get("/guidance-counseling/gc-corner-article-personal-well-being")]
        Task<ApiErrorResult<IEnumerable<GetGcCornerWellBeingResult>>> GetListGcCornerArticlePersonalWellBeing(GetGcCornerWellBeingRequest query);

        [Get("/guidance-counseling/gc-corner-gc-link")]
        Task<ApiErrorResult<IEnumerable<GetGcCornerGcLinkResult>>> GetListGcCornerGcLink(GetGcCornerGcLinkRequest query);

        [Get("/guidance-counseling/gc-corner-country-fact")]
        Task<ApiErrorResult<IEnumerable<GetGcCornerCountryFactResult>>> GetListGcCornerCountryFact(GetGcCornerCountryFactRequest query);

        [Get("/guidance-counseling/gc-corner-useful-link")]
        Task<ApiErrorResult<IEnumerable<GetGcCornerUsefulLinkResult>>> GetListGcCornerUsefulLink(GetGcCornerUsefulLinkRequest query);

        [Get("/guidance-counseling/gc-corner-university-portal")]
        Task<ApiErrorResult<IEnumerable<GetGcCornerUniversityPortalResult>>> GetListGcCornerUniversityPortal(GetGcCornerUniversityPortalRequest query);

        [Get("/guidance-counseling/gc-corner-your-counselor")]
        Task<ApiErrorResult<GetGcCornerYourCounselorResult>> GetGcCornerYourCounselor(GetGcCornerYourCounselorRequest query);
    }
}
