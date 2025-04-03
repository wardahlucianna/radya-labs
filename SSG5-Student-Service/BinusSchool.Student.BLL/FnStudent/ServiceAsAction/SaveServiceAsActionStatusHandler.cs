using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class SaveServiceAsActionStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveServiceAsActionStatusHandler
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
                var param = await Request.ValidateBody<SaveServiceAsActionStatusRequest, SaveServiceAsActionStatusValidator>();

                var dataExperience = await _dbContext.Entity<TrServiceAsActionForm>()
                    .Include(x => x.ServiceAsActionStatus)
                    .Where(x => x.Id == param.IdServiceAsActionForm)
                    .FirstOrDefaultAsync(CancellationToken);

                if(dataExperience == null ) throw new Exception("Data Not Found");

                if(dataExperience.ServiceAsActionStatus.StatusDesc == "Complete") throw new Exception("Experience Already Completed");

                if (String.IsNullOrEmpty(param.RevisionNote))
                {
                    dataExperience.IdServiceAsActionStatus = param.IdServiceAsActionStatus;
                    dataExperience.ApprovedBy = AuthInfo.UserId;
                    dataExperience.ApprovedDate = DateTime.Now;
                    _dbContext.Entity<TrServiceAsActionForm>().Update(dataExperience);
                }
                else
                {
                    dataExperience.RevisionNote = param.RevisionNote;
                    dataExperience.IdServiceAsActionStatus = param.IdServiceAsActionStatus;
                    dataExperience.ApprovedBy = AuthInfo.UserId;
                    dataExperience.ApprovedDate = DateTime.Now;
                    _dbContext.Entity<TrServiceAsActionForm>().Update(dataExperience);
                }
                

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
