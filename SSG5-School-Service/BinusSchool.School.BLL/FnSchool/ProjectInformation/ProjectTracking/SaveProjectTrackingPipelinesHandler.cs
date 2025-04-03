using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.ProjectInformation.Helper;
using BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class SaveProjectTrackingPipelinesHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _context;
        private readonly IProjectInformation _projectInformation;
        private IDbContextTransaction _transaction;

        public SaveProjectTrackingPipelinesHandler(ISchoolDbContext context, IProjectInformation projectInformation)
        {
            _context = context;
            _projectInformation = projectInformation;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<SaveProjectTrackingPipelinesRequest, SaveProjectTrackingPipelinesValidator>();

            await SaveProjectTrackingPipelines(request);

            return Request.CreateApiResult2(code: System.Net.HttpStatusCode.Created);
        }

        public async Task SaveProjectTrackingPipelines(SaveProjectTrackingPipelinesRequest request)
        {
            using (_transaction = await _context.BeginTransactionAsync(CancellationToken, System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var accessRoleApi = await _projectInformation.GetProjectInformationRoleAccess();
                    bool checkRole = accessRoleApi.Payload.AllowCreateUpdateDelete;

                    if (!checkRole)
                        throw new BadRequestException($"Access denied!");

                    if (string.IsNullOrEmpty(request.IdProjectPipeline))
                    {
                        var insertProjectPipeline = new MsProjectPipeline
                        {
                            Id = Guid.NewGuid().ToString(),
                            Year = request.StartDate.Year.ToString(),
                            IdProjectSection = request.IdSection,
                            SprintName = request.SprintName,
                            Description = request.Description,
                            StartDate = request.StartDate,
                            EndDate = request.EndDate,
                            IdProjectStatus = request.IdStatus,
                            IdProjectPhase = request.IdPhase,
                        };

                        _context.Entity<MsProjectPipeline>().Add(insertProjectPipeline);
                    }
                    else
                    {
                        var projectPipeline = await _context.Entity<MsProjectPipeline>()
                            .Where(a => a.Id == request.IdProjectPipeline)
                            .FirstOrDefaultAsync();

                        projectPipeline.Year = request.StartDate.Year.ToString();
                        projectPipeline.IdProjectSection = request.IdSection;
                        projectPipeline.SprintName = request.SprintName;
                        projectPipeline.Description = request.Description;
                        projectPipeline.StartDate = request.StartDate;
                        projectPipeline.EndDate = request.EndDate;
                        projectPipeline.IdProjectStatus = request.IdStatus;
                        projectPipeline.IdProjectPhase = request.IdPhase;

                        _context.Entity<MsProjectPipeline>().Update(projectPipeline);
                    }

                    await _context.SaveChangesAsync();
                    await _transaction.CommitAsync();
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
