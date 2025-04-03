using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.ServiceAsAction.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.ServiceAsAction
{
    public class DeleteExperienceServiceAsActionHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteExperienceServiceAsActionHandler
        (
            IStudentDbContext studentDbContext
        )
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteExperienceServiceAsActionRequest, DeleteExperienceServiceAsActionValidator>();

            var getServiceAsActionForm = await _dbContext.Entity<TrServiceAsActionForm>()
                .Where(x => x.Id == param.IdServiceAsActionForm)
                .FirstOrDefaultAsync(CancellationToken);

            if (getServiceAsActionForm == null) throw new BadRequestException($"Data Not Found");

            #region Get Data

            // Check Mappinng Learning Outcome
            var getMappedLearningOutcomes = await _dbContext.Entity<TrServiceAsActionMappingForm>()
                .Where(x => x.IdServiceAsActionForm == param.IdServiceAsActionForm)
                .ToListAsync(CancellationToken);

            // Check Mapping Type
            var getMappedTypes = await _dbContext.Entity<TrServiceAsActionMappingType>()
                .Where(x => x.IdServiceAsActionForm == param.IdServiceAsActionForm)
                .ToListAsync(CancellationToken);

            // Check Evidences
            var getEvidences = await _dbContext.Entity<TrServiceAsActionEvidence>()
                .Where(x => x.IdServiceAsActionForm == param.IdServiceAsActionForm)
                .ToListAsync(CancellationToken);

            // Check Evidence Mapping Learning Outcome
            var getEvidenceMappedLearningOutcomes = await _dbContext.Entity<TrServiceAsActionMapping>()
                .Where(x =>getEvidences.Select(y => y.Id).ToList().Any(y => y == x.IdServiceAsActionEvidence))
                .ToListAsync(CancellationToken);

            // Check Evidence Upload
            var getEvidenceUploads = await _dbContext.Entity<TrServiceAsActionUpload>()
                .Where(x => getEvidences.Select(y => y.Id).ToList().Any(y => y == x.IdServiceAsActionEvidence))
                .ToListAsync(CancellationToken);

            // Check Evidence Comments
            var getEvidenceComments = await _dbContext.Entity<TrServiceAsActionComment>()
                .Where(x => getEvidences.Select(y => y.Id).ToList().Any(y => y == x.IdServiceAsActionEvidence))
                .ToListAsync(CancellationToken);

            #endregion

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                if(getEvidences.Count > 0)
                {
                    if(getEvidenceMappedLearningOutcomes.Count > 0)
                    {
                        foreach (var evidenceLO in getEvidenceMappedLearningOutcomes)
                        {
                            evidenceLO.IsActive = false;
                        }

                        _dbContext.Entity<TrServiceAsActionMapping>().UpdateRange(getEvidenceMappedLearningOutcomes);
                    }

                    if (getEvidenceUploads.Count > 0)
                    {
                        foreach(var evidenceUpload in getEvidenceUploads)
                        {
                            evidenceUpload.IsActive = false;
                        }
                        _dbContext.Entity<TrServiceAsActionUpload>().UpdateRange(getEvidenceUploads);
                    }

                    if (getEvidenceComments.Count > 0)
                    {
                        foreach (var evidenceComment in getEvidenceComments)
                        {
                            evidenceComment.IsActive = false;
                        }
                        _dbContext.Entity<TrServiceAsActionComment>().UpdateRange(getEvidenceComments);
                    }

                    foreach (var evidence in getEvidences)
                    {
                        evidence.IsActive = false;
                    }
                    _dbContext.Entity<TrServiceAsActionEvidence>().UpdateRange(getEvidences);
                }

                if(getMappedLearningOutcomes.Count > 0)
                {
                    foreach (var mappedLO in getMappedLearningOutcomes)
                    {
                        mappedLO.IsActive = false;
                    }
                    _dbContext.Entity<TrServiceAsActionMappingForm>().UpdateRange(getMappedLearningOutcomes);
                }

                if (getMappedTypes.Count > 0)
                {
                    foreach (var mappedType in getMappedTypes)
                    {
                        mappedType.IsActive = false;
                    }
                    _dbContext.Entity<TrServiceAsActionMappingType>().UpdateRange(getMappedTypes);
                }

                getServiceAsActionForm.IsActive = false;

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }

            return Request.CreateApiResult2(null as object);
        }
    }
}
