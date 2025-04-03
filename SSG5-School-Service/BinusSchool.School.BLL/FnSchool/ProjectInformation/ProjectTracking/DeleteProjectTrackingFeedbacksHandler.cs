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
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking
{
    public class DeleteProjectTrackingFeedbacksHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _context;
        private readonly IProjectInformation _projectInformation;
        private IDbContextTransaction _transaction;

        public DeleteProjectTrackingFeedbacksHandler(ISchoolDbContext context, IProjectInformation projectInformation)
        {
            _context = context;
            _projectInformation = projectInformation;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<DeleteProjectTrackingFeedbacksRequest, DeleteProjectTrackingFeedbacksValidator>();

            await DeleteProjectTrackingFeedbacks(request);

            return Request.CreateApiResult2(code: System.Net.HttpStatusCode.NoContent);
        }

        public async Task DeleteProjectTrackingFeedbacks(DeleteProjectTrackingFeedbacksRequest request)
        {
            using (_transaction = await _context.BeginTransactionAsync(CancellationToken, System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var accessRole = await _projectInformation.GetProjectInformationRoleAccess();

                    var accessCheck = accessRole.Payload.AllowCreateUpdateDelete;

                    if (!accessCheck)
                        throw new BadRequestException($"Access denied!");

                    var projectMappings = await _context.Entity<MsProjectFeedbackSprintMapping>()
                        .Where(a => a.IdProjectFeedback == request.IdProjectFeedback)
                        .ToListAsync(CancellationToken);

                    foreach (var mapping in projectMappings)
                    {
                        mapping.IsActive = false;

                        _context.Entity<MsProjectFeedbackSprintMapping>().Update(mapping);
                    }

                    var projectFeedback = await _context.Entity<MsProjectFeedback>()
                        .Where(a => a.Id == request.IdProjectFeedback)
                        .FirstOrDefaultAsync(CancellationToken);

                    projectFeedback.IsActive = false;

                    _context.Entity<MsProjectFeedback>().Update(projectFeedback);

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
