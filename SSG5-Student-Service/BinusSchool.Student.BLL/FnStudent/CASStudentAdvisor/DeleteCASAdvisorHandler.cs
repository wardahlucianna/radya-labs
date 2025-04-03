using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.CASStudentAdvisor.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.CASStudentAdvisor
{
    public class DeleteCASAdvisorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteCASAdvisorHandler(
                IStudentDbContext dbContext
            )
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteCASAdvisorRequest, DeleteCASAdvisorValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var dataToDelete = await _dbContext.Entity<TrCasAdvisor>()
                                .Include(x => x.TrCasAdvisorStudents)
                                .Where(x => x.Id == param.IdCasAdvisor)
                                .FirstOrDefaultAsync(CancellationToken);

                if(dataToDelete.TrCasAdvisorStudents.Count() == 0)
                {
                    dataToDelete.IsActive = false;
                    _dbContext.Entity<TrCasAdvisor>().Update(dataToDelete);
                }
                else
                {
                    throw new BadRequestException("$There are still students who are mapped with the advisor.");
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
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
        }

    }
}
