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
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class GetProjectTrackingSectionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _context;

        public GetProjectTrackingSectionHandler(ISchoolDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.GetParams<GetProjectTrackingSectionRequest>();

            var response = await GetProjectTrackingSection(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<List<GetProjectTrackingSectionResponse>> GetProjectTrackingSection(GetProjectTrackingSectionRequest request)
        {
            var response = new List<GetProjectTrackingSectionResponse>();

            var getProjectSection = await _context.Entity<LtProjectSection>()
                .ToListAsync(CancellationToken);

            if (!string.IsNullOrEmpty(request.Search))
                getProjectSection = getProjectSection
                    .Where(a => EF.Functions.Like(a.Code, request.SearchPattern())
                        || EF.Functions.Like(a.Description, request.SearchPattern()))
                    .ToList();

            var data = getProjectSection
                .Select(a => new GetProjectTrackingSectionResponse
                {
                    Id = a.Id,
                    Code = a.Code,
                    Description = a.Description,
                })
                .OrderBy(a => a.Description)
                .ToList();

            response.AddRange(data);

            return response;
        }
    }
}
