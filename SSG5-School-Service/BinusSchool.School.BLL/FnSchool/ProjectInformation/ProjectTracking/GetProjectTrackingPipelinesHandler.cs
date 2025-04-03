using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class GetProjectTrackingPipelinesHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _context;
        private static readonly string[] _columns = new[]
        {
            "Section",
            "SprintName",
            "ProjectDescription",
            "PlannedDate",
            "Status",
            "Phase"
        };

        public GetProjectTrackingPipelinesHandler(ISchoolDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetProjectTrackingPipelinesRequest>(
                nameof(GetProjectTrackingPipelinesRequest.Year));
            
            var response = await GetProjectTrackingPipelines(request);

            var count = request.CanCountWithoutFetchDb(response.Count)
                ? response.Count
                : response.Select(a => a.IdProjectPipeline).Count();

            response = response
                .SetPagination(request)
                .ToList();

            return Request.CreateApiResult2(response as object, request.CreatePaginationProperty(count));
        }

        public async Task<List<GetProjectTrackingPipelinesResponse>> GetProjectTrackingPipelines(GetProjectTrackingPipelinesRequest request)
        {
            var response = new List<GetProjectTrackingPipelinesResponse>();

            var getProjectPipelines = await _context.Entity<MsProjectPipeline>()
                .Include(a => a.ProjectPhase)
                .Include(a => a.ProjectSection)
                .Include(a => a.ProjectStatus)
                .Where(a => a.Year == request.Year
                    && (string.IsNullOrEmpty(request.IdSection) ? true : a.IdProjectSection == request.IdSection)
                    && (string.IsNullOrEmpty(request.IdStatus) ? true : a.IdProjectStatus == request.IdStatus))
                .ToListAsync(CancellationToken);

            var data = getProjectPipelines
                .Select(a => new GetProjectTrackingPipelinesResponse
                {
                    IdProjectPipeline = a.Id,
                    Section = new ItemValueVm
                    {
                        Id = a.ProjectSection.Id,
                        Description = a.ProjectSection.Description,
                    },
                    SprintName = a.SprintName,
                    Description = a.Description,
                    StartDate = a.StartDate.Date,
                    EndDate = a.EndDate.Date,
                    Status = new ItemValueVm
                    {
                        Id = a.ProjectStatus.Id,
                        Description = a.ProjectStatus.Description,
                    },
                    Phase = new ItemValueVm
                    {
                        Id = a.ProjectPhase.Id,
                        Description = a.ProjectPhase.Description,
                    }
                })
                .ToList();

            if (!data.Any())
                return response;

            if (!string.IsNullOrEmpty(request.Search))
                data = data
                    .Where(a => EF.Functions.Like(a.Section.Description, request.SearchPattern())
                        || EF.Functions.Like(a.SprintName, request.SearchPattern())
                        || EF.Functions.Like(a.Description, request.SearchPattern())
                        || EF.Functions.Like(a.StartDate.Date.ToString(), request.SearchPattern())
                        || EF.Functions.Like(a.EndDate.Date.ToString(), request.SearchPattern())
                        || EF.Functions.Like(a.Section.Description, request.SearchPattern())
                        || EF.Functions.Like(a.Phase.Description, request.SearchPattern()))
                    .ToList();

            response.AddRange(data);

            response = request.OrderBy switch
            {
                "Section" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Section.Description).ToList()
                    : response.OrderByDescending(a => a.Section.Description).ToList(),
                "SprintName" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.SprintName).ToList()
                    : response.OrderByDescending(a => a.SprintName).ToList(),
                "ProjectDescription" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Description).ToList()
                    : response.OrderByDescending(a => a.Description).ToList(),
                "PlannedDate" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.StartDate).ToList()
                    : response.OrderByDescending(a => a.StartDate).ToList(),
                "Status" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Status.Description).ToList()
                    : response.OrderByDescending(a => a.Status.Description).ToList(),
                "Phase" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Phase.Description).ToList()
                    : response.OrderByDescending(a => a.Phase.Description).ToList(),
                _ => response.OrderBy(a => a.Section.Description).ToList()
            };

            return response;
        }
    }
}
