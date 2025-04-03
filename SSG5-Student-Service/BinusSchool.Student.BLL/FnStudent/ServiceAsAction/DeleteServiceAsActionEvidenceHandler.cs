using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.ServiceAsAction.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.ServiceAsAction
{
    public class DeleteServiceAsActionEvidenceHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteServiceAsActionEvidenceHandler
        (
            IStudentDbContext studentDbContext
        )
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {

                var param = await Request.ValidateBody<DeleteServiceAsActionEvidenceRequest, DeleteServiceAsActionEvidenceValidator>();

                var dataEvidence = await _dbContext.Entity<TrServiceAsActionEvidence>()
                    .Include(x => x.ServiceAsActionForm)
                        .ThenInclude(x => x.ServiceAsActionStatus)
                    .Where(x => x.Id == param.IdServiceAsActionEvidence)
                    .FirstOrDefaultAsync(CancellationToken);

                if( dataEvidence.ServiceAsActionForm.ServiceAsActionStatus.StatusDesc == "Complete") throw new Exception("Experience Already Completed");
                if (dataEvidence == null) throw new Exception("Evidence Not Found");

                var getEvidenceMapping = await _dbContext.Entity<TrServiceAsActionMapping>()
                    .Where(x => x.IdServiceAsActionEvidence == param.IdServiceAsActionEvidence)
                    .ToListAsync(CancellationToken);

                var getUploads = await _dbContext.Entity<TrServiceAsActionUpload>()
                    .Where(x => x.IdServiceAsActionEvidence == param.IdServiceAsActionEvidence)
                    .ToListAsync(CancellationToken);

                if(getEvidenceMapping.Count > 0)
                {
                    foreach(var item in getEvidenceMapping)
                    {
                        item.IsActive = false;
                        _dbContext.Entity<TrServiceAsActionMapping>().Update(item);
                    }
                }

                if (getUploads.Count > 0)
                {
                    foreach (var item in getUploads)
                    {
                        item.IsActive = false;
                        _dbContext.Entity<TrServiceAsActionUpload>().Update(item);
                    }
                }

                dataEvidence.IsActive = false;
                _dbContext.Entity<TrServiceAsActionEvidence>().Update(dataEvidence);

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

            return Request.CreateApiResult2();
        }
    }
}
