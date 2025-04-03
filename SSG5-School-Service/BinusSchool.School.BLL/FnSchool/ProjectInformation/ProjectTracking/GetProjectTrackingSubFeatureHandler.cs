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
    public class GetProjectTrackingSubFeatureHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _context;

        public GetProjectTrackingSubFeatureHandler(ISchoolDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetProjectTrackingSubFeatureRequest>(
                nameof(GetProjectTrackingSubFeatureRequest.IdFeature));

            var response = await GetProjectTrackingSubFeature(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<List<GetProjectTrackingSubFeatureResponse>> GetProjectTrackingSubFeature(GetProjectTrackingSubFeatureRequest request)
        {
            var response = new List<GetProjectTrackingSubFeatureResponse>();

            var subFeatures = await _context.Entity<MsFeature>()
                .Where(a => !(string.IsNullOrEmpty(a.IdParent))
                    && a.IdParent == request.IdFeature)
                .ToListAsync(CancellationToken);

            if (!subFeatures.Any())
                return response;

            var data = subFeatures
                .Select(a => new GetProjectTrackingSubFeatureResponse
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
