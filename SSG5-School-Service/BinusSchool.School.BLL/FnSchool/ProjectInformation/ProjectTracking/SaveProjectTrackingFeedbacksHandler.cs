using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class SaveProjectTrackingFeedbacksHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _context;
        private readonly IProjectInformation _projectInformation;
        private IDbContextTransaction _transaction;

        public SaveProjectTrackingFeedbacksHandler(ISchoolDbContext context, IProjectInformation projectInformation)
        {
            _context = context;
            _projectInformation = projectInformation;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<SaveProjectTrackingFeedbacksRequest, SaveProjectTrackingFeedbacksValidator>();

            await SaveProjectTrackingFeedbacks(request);

            return Request.CreateApiResult2(code: System.Net.HttpStatusCode.Created);
        }

        public async Task SaveProjectTrackingFeedbacks(SaveProjectTrackingFeedbacksRequest request)
        {
            using (_transaction = await _context.BeginTransactionAsync(CancellationToken, System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var accessRole = await _projectInformation.GetProjectInformationRoleAccess();

                    var accessCheck = accessRole.Payload.AllowCreateUpdateDelete;

                    if (!accessCheck)
                        throw new BadRequestException($"Access denied!");

                    if (string.IsNullOrEmpty(request.IdProjectFeedback))
                    {
                        var newId = Guid.NewGuid().ToString();
                        var insertData = new MsProjectFeedback
                        {
                            Id = newId,
                            RequestDate = request.RequestDate.Date,
                            IdSchool = request.IdSchool,
                            IdBinusian = request.Requester,
                            FeatureRequested = request.FeatureRequested,
                            Description = request.Description,
                            IdRelatedModule = request.IdRelatedModule,
                            IdRelatedSubModule = request.IdRelatedSubModule,
                            IdProjectStatus = request.IdStatus,
                        };

                        _context.Entity<MsProjectFeedback>().Add(insertData);

                        if (request.SprintPlanned != null && request.SprintPlanned.Count > 0)
                        {
                            foreach (var sprintPlanned in request.SprintPlanned)
                            {
                                var insertMappingData = new MsProjectFeedbackSprintMapping
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdProjectFeedback = newId,
                                    IdProjectPipeline = sprintPlanned.IdProjectPipeline,
                                };

                                _context.Entity<MsProjectFeedbackSprintMapping>().Add(insertMappingData);
                            }
                        }
                    }
                    else
                    {
                        var projectFeedback = await _context.Entity<MsProjectFeedback>()
                            .Where(a => a.Id == request.IdProjectFeedback)
                            .FirstOrDefaultAsync(CancellationToken);

                        projectFeedback.RequestDate = request.RequestDate;
                        projectFeedback.FeatureRequested = request.FeatureRequested;
                        projectFeedback.Description = request.Description;
                        projectFeedback.IdRelatedModule = request.IdRelatedModule;
                        projectFeedback.IdRelatedSubModule = request.IdRelatedSubModule;
                        projectFeedback.IdProjectStatus = request.IdStatus;

                        var projectFeedbackSprintMappings = await _context.Entity<MsProjectFeedbackSprintMapping>()
                            .Where(a => a.IdProjectFeedback == request.IdProjectFeedback)
                            .ToListAsync(CancellationToken);

                        var addProjectFeedbackSprintMappings = request.SprintPlanned
                            .Where(a => projectFeedbackSprintMappings.All(b => b.IdProjectPipeline != a.IdProjectPipeline))
                            .Select(a => a.IdProjectPipeline)
                            .ToList();

                        var deleteProjectFeedbackSprintMappings = projectFeedbackSprintMappings
                            .Where(a => request.SprintPlanned.All(b => b.IdProjectPipeline != a.IdProjectPipeline))
                            .Select(a => a.IdProjectPipeline)
                            .ToList();

                        // delete mapping
                        foreach (var delete in deleteProjectFeedbackSprintMappings)
                        {
                            var deleteMapping = projectFeedbackSprintMappings
                                .Where(a => a.IdProjectPipeline == delete)
                                .FirstOrDefault();

                            if (deleteMapping != null)
                                deleteMapping.IsActive = false;

                            _context.Entity<MsProjectFeedbackSprintMapping>().Update(deleteMapping);
                        }

                        // add mapping
                        foreach (var insert in addProjectFeedbackSprintMappings)
                        {
                            var insertMapping = new MsProjectFeedbackSprintMapping
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdProjectFeedback = projectFeedback.Id,
                                IdProjectPipeline = insert
                            };

                            _context.Entity<MsProjectFeedbackSprintMapping>().Add(insertMapping);
                        }
                    }

                    await _context.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);
                }
                catch (Exception ex)
                {
                    _transaction?.Rollback();

                    throw new Exception(ex.Message.ToString());
                }
            }
        }
    }
}
