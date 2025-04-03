using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.CASStudentAdvisor.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.CASStudentAdvisor
{
    public class AddCASAdvisorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public AddCASAdvisorHandler(
                IStudentDbContext dbContext
            )
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<AddCASAdvisorRequest, AddCASAdvisorValidator>();

            var advisorList = await _dbContext.Entity<TrCasAdvisor>()
                            .Where(x => x.IdAcademicYear == param.IdAcademicYear && x.IdUserCAS == param.IdUser)
                            .FirstOrDefaultAsync(CancellationToken);
            try
            {

                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                if (advisorList == null)
                {
                    var insert = new TrCasAdvisor
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdUserCAS = param.IdUser,
                        IdAcademicYear = param.IdAcademicYear,
                    };

                    _dbContext.Entity<TrCasAdvisor>().Add(insert);
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
