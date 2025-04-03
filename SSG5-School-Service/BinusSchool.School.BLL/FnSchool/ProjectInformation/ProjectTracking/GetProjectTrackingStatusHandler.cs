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
    public class GetProjectTrackingStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _context;

        public GetProjectTrackingStatusHandler(ISchoolDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.GetParams<GetProjectTrackingStatusRequest>();

            var response = await GetProjectTrackingStatus(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<List<GetProjectTrackingStatusResponse>> GetProjectTrackingStatus(GetProjectTrackingStatusRequest request)
        {
            var response = new List<GetProjectTrackingStatusResponse>();

            var getProjectStatus = await _context.Entity<LtProjectStatus>()
                .ToListAsync(CancellationToken);

            if (!string.IsNullOrEmpty(request.Search))
                getProjectStatus = getProjectStatus
                    .Where(a => EF.Functions.Like(a.Code, request.SearchPattern())
                        || EF.Functions.Like(a.Description, request.SearchPattern()))
                    .ToList();

            var data = getProjectStatus
                .Select(a => new GetProjectTrackingStatusResponse
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
