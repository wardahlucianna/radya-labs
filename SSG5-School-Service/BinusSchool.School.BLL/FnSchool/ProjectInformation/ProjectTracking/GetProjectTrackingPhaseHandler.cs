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
    public class GetProjectTrackingPhaseHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _context;

        public GetProjectTrackingPhaseHandler(ISchoolDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.GetParams<GetProjectTrackingPhaseRequest>();

            var response = await GetProjectTrackingPhase(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<List<GetProjectTrackingPhaseResponse>> GetProjectTrackingPhase(GetProjectTrackingPhaseRequest request)
        {
            var response = new List<GetProjectTrackingPhaseResponse>();

            var projectPhases = await _context.Entity<LtProjectPhase>()
                .ToListAsync(CancellationToken);

            if (!string.IsNullOrEmpty(request.Search))
                projectPhases = projectPhases
                    .Where(a => EF.Functions.Like(a.Description, request.SearchPattern())
                        || EF.Functions.Like(a.Code, request.SearchPattern()))
                    .ToList();

            var data = projectPhases
                .Select(a => new GetProjectTrackingPhaseResponse
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
