using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalHospital;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MedicalSystem.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalHospital
{
    public class DeleteMedicalHospitalHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteMedicalHospitalHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteMedicalHospitalRequest, DeleteMedicalHospitalValidator>();

            try
            {

                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var data = await _dbContext.Entity<MsMedicalHospital>()
                    .Include(x => x.MedicalDoctors)
                    .Include(x => x.MedicalRecordEntries)
                    .Where(x => param.IdHospital.Contains(x.Id))
                    .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical Hospital not found");

                if (data.MedicalRecordEntries.Any() || data.MedicalDoctors.Any())
                {
                    throw new BadRequestException("Medical Hospital cannot be deleted because it is used in other data");
                }

                data.IsActive = false;

                _dbContext.Entity<MsMedicalHospital>().Update(data);

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
