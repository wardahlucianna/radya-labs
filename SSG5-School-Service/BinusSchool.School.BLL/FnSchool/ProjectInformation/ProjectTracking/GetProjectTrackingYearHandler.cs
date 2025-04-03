using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class GetProjectTrackingYearHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _context;
        private readonly IMachineDateTime _date;

        public GetProjectTrackingYearHandler(ISchoolDbContext context, IMachineDateTime date)
        {
            _context = context;
            _date = date;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetProjectTrackingYearRequest>(
                nameof(GetProjectTrackingYearRequest.Type));

            var response = await GetProjectTrackingYear(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<List<GetProjectTrackingYearResponse>> GetProjectTrackingYear(GetProjectTrackingYearRequest request)
        {
            var response = new List<GetProjectTrackingYearResponse>();

            var currentYear = _date.ServerTime.Year.ToString();

            var getProjectPipeline = await _context.Entity<MsProjectPipeline>()
                .Select(a => new { Id = a.Year, Code = a.Year })
                .Distinct()
                .ToListAsync();

            var getProjectFeedback = await _context.Entity<MsProjectFeedback>()
                .Select(a => new { Id = a.RequestDate.Year.ToString(), Code = a.RequestDate.Year.ToString() })
                .Distinct()
                .ToListAsync();

            if (request.Type.ToLower() == "pipeline")
            {
                if (getProjectPipeline.Any())
                {
                    if (!getProjectPipeline.Any(a => a.Code == currentYear))
                    {
                        getProjectPipeline.Add(new { Id = currentYear, Code = currentYear });
                    }

                    response = getProjectPipeline
                        .Select(a => new GetProjectTrackingYearResponse
                        {
                            Id = a.Id,
                            Description = a.Code,
                        })
                        .OrderByDescending(a => a.Description)
                        .ToList();
                }
                else
                {
                    response = new List<GetProjectTrackingYearResponse>
                    {
                        new GetProjectTrackingYearResponse
                        {
                            Id = currentYear,
                            Description = currentYear
                        }
                    };
                }
            }
            else 
            {
                if (getProjectFeedback.Any())
                {
                    if (!getProjectFeedback.Any(a => a.Code == currentYear))
                    {
                        getProjectFeedback.Add(new { Id = currentYear, Code = currentYear });
                    }

                    response = getProjectFeedback
                        .Select(a => new GetProjectTrackingYearResponse
                        {
                            Id = a.Id,
                            Description = a.Code,
                        })
                        .OrderByDescending(a => a.Description)
                        .ToList();
                }
                else
                {
                    response = new List<GetProjectTrackingYearResponse>
                    {
                        new GetProjectTrackingYearResponse
                        {
                            Id = currentYear,
                            Description = currentYear
                        }
                    };
                }
            }

            if (!string.IsNullOrEmpty(request.Search))
                response = response
                    .Where(a => EF.Functions.Like(a.Id, request.SearchPattern())
                        || EF.Functions.Like(a.Description, request.SearchPattern()))
                    .ToList();

            return response;
        }
    }
}
