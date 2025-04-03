using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class GetProjectTrackingFeedbacksHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _context;

        private static readonly string[] _columns = new[]
        {
            "School",
            "RequestDate",
            "Requester",
            "FeatureRequested",
            "Status"
        };

        public GetProjectTrackingFeedbacksHandler(ISchoolDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetProjectTrackingFeedbacksRequest>(
                nameof(GetProjectTrackingFeedbacksRequest.Year));

            var response = await GetProjectTrackingFeedbacks(request);

            var count = request.CanCountWithoutFetchDb(response.Count)
                ? response.Count
                : response.Select(a => a.IdProjectFeedback).Count();

            response = response
                .SetPagination(request)
                .ToList();

            return Request.CreateApiResult2(response as object, request.CreatePaginationProperty(count));
        }

        public async Task<List<GetProjectTrackingFeedbacksResponse>> GetProjectTrackingFeedbacks(GetProjectTrackingFeedbacksRequest request)
        {
            var response = new List<GetProjectTrackingFeedbacksResponse>();

            var projectFeedbacks = await _context.Entity<MsProjectFeedback>()
                .Include(a => a.ProjectFeedbackSprintMappings)
                    .ThenInclude(b => b.ProjectPipeline.ProjectSection)
                .Include(a => a.School)
                .Include(a => a.Staff)
                .Include(a => a.ProjectStatus)
                .Include(a => a.RelatedModule)
                .Include(a => a.RelatedSubModule)
                .Where(a => a.RequestDate.Year.ToString() == request.Year
                    && (string.IsNullOrEmpty(request.IdSchool) ? true : a.IdSchool == request.IdSchool)
                    && (string.IsNullOrEmpty(request.IdStatus) ? true : a.IdProjectStatus == request.IdStatus))
                .ToListAsync(CancellationToken);

            var data = projectFeedbacks
                .Select(a => new GetProjectTrackingFeedbacksResponse
                {
                    IdProjectFeedback = a.Id,
                    School = new ItemValueVm
                    {
                        Id = a.School.Id,
                        Description = a.School.Name
                    },
                    RequestDate = a.RequestDate.Date,
                    Requester = new ItemValueVm
                    {
                        Id = a.Staff.IdBinusian,
                        Description = NameUtil.GenerateFullName(a.Staff.FirstName, a.Staff.LastName),
                    },
                    FeatureRequested = a.FeatureRequested,
                    Description = a.Description,
                    Status = new ItemValueVm
                    {
                        Id = a.ProjectStatus.Id,
                        Description = a.ProjectStatus.Description,
                    },
                    RelatedModule = a.RelatedModule != null
                        ? new ItemValueVm
                        {
                            Id = a.RelatedModule.Id,
                            Description = a.RelatedModule.Description,
                        }
                        : null,

                        RelatedSubModule = a.RelatedSubModule != null
                        ? new ItemValueVm
                        {
                            Id = a.RelatedSubModule.Id,
                            Description = a.RelatedSubModule.Description,
                        }
                        : null,
                    SprintPlanned = a.ProjectFeedbackSprintMappings
                        .SelectMany(b => new[] { new GetProjectTrackingFeedbacksResponse_SprintPlanned
                        {
                            IdProjectFeedbackSprintMapping = b.Id,
                            Year = b.ProjectPipeline.Year.ToString(),
                            Section = new ItemValueVm
                            {
                                Id = b.ProjectPipeline.ProjectSection.Id,
                                Description = b.ProjectPipeline.ProjectSection.Description,
                            },
                            SprintName = new ItemValueVm
                            {
                                Id = b.ProjectPipeline.Id,
                                Description = b.ProjectPipeline.SprintName
                            },
                            StartDate = b.ProjectPipeline.StartDate.Date,
                            EndDate = b.ProjectPipeline.EndDate.Date,
                        }})
                        .OrderBy(a => a.StartDate.Date)
                            .ThenBy(a => a.SprintName.Description)
                        .ToList()
                })
                .ToList();

            if (!data.Any())
                return response;

            if (!string.IsNullOrEmpty(request.Search))
                data = data
                    .Where(a => EF.Functions.Like(a.School.Description, request.SearchPattern())
                        || EF.Functions.Like(a.RequestDate.Date.ToString(), request.SearchPattern())
                        || EF.Functions.Like(a.Requester.Description, request.SearchPattern())
                        || EF.Functions.Like(a.FeatureRequested, request.SearchPattern())
                        || EF.Functions.Like(a.Status.Description, request.SearchPattern()))
                    .ToList();

            response.AddRange(data);

            response = request.OrderBy switch
            {
                "School" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.School.Description).ToList()
                    : response.OrderByDescending(a => a.School.Description).ToList(),
                "RequestDate" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.RequestDate).ToList()
                    : response.OrderByDescending(a => a.RequestDate).ToList(),
                "Requester" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Requester.Description).ToList()
                    : response.OrderByDescending(a => a.Requester.Description).ToList(),
                "FeatureRequested" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.FeatureRequested).ToList()
                    : response.OrderByDescending(a => a.FeatureRequested).ToList(),
                "Status" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Status.Description).ToList()
                    : response.OrderByDescending(a => a.Status.Description).ToList(),
                _ => response.OrderBy(a => a.School.Description).ToList()
            };

            return response;
        }
    }
}
