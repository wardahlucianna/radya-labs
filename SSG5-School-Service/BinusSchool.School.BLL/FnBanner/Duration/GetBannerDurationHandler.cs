
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnBanner.Duration;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnBanner.Duration
{
    public class GetBannerDurationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetBannerDurationHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetBannerDurationRequest>(nameof(GetBannerDurationRequest.Type));

            var durationSetting = await _dbContext.Entity<MsDurationSetting>()
                .Where(x => x.Type == param.Type)
                .Select(x => new GetBannerDurationResult
                {
                    Type = x.Type,
                    Duration = x.Duration
                })
                .FirstOrDefaultAsync();

            return Request.CreateApiResult2(durationSetting as object);
        }
    }
}
