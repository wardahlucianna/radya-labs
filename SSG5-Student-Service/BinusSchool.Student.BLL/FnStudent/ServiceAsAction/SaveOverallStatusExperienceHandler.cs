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
    public class SaveOverallStatusExperienceHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveOverallStatusExperienceHandler
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
                var param = await Request.ValidateBody<SaveOverallStatusExperienceRequest, SaveOverallStatusExperienceValidator>();

                var dataHeader = await _dbContext.Entity<TrServiceAsActionHeader>()
                    .Where(x => x.IdStudent == param.IdStudent && x.IdAcademicYear == param.IdAcademicYear)
                    .FirstOrDefaultAsync(CancellationToken);

                if (dataHeader == null) throw new Exception("Data Not Found");

                if(dataHeader.IdStatusOverall == param.IdServiceAsActionStatus)
                {
                    throw new Exception("Status Already Set");
                }

                dataHeader.IdStatusOverall = param.IdServiceAsActionStatus;
                _dbContext.Entity<TrServiceAsActionHeader>().Update(dataHeader);

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
