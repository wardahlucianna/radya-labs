using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class GetProjectTrackingFeatureHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _context;

        public GetProjectTrackingFeatureHandler(ISchoolDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.GetParams<GetProjectTrackingFeatureRequest>();

            var response = await GetProjectTrackingFeature(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<List<GetProjectTrackingFeatureResponse>> GetProjectTrackingFeature(GetProjectTrackingFeatureRequest request)
        {
            var response = new List<GetProjectTrackingFeatureResponse>();

            var features = await _context.Entity<MsFeature>()
                .Where(a => (string.IsNullOrEmpty(a.IdParent)))
                .ToListAsync(CancellationToken);

            if (!features.Any())
                return response;

            var data = features
                .Select(a => new GetProjectTrackingFeatureResponse
                {
                    Id = a.Id,
                    Description = a.IsShowMobile == true ? $"{a.Description} (Mobile)" : a.Description,
                })
                .OrderBy(a => a.Description)
                .ToList();

            if (!string.IsNullOrEmpty(request.Search))
                data = data
                    .Where(a => EF.Functions.Like(a.Description, request.SearchPattern()))
                    .ToList();

            response.AddRange(data);

            return response;
        }
    }
}
