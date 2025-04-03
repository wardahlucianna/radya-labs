using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnBanner.Duration;
using Refit;

namespace BinusSchool.Data.Api.School.FnBanner
{
    public interface IDuration : IFnBanner
    {

        [Get("/banner/duration")]
        Task<ApiErrorResult<GetBannerDurationResult>> GetDurationBanner(GetBannerDurationRequest param);
    }
}
