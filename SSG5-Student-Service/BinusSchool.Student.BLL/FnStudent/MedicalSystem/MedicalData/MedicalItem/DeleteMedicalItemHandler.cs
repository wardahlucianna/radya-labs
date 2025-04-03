using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MedicalSystem.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalItem
{
    public class DeleteMedicalItemHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteMedicalItemHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<IdCollection, DeleteMedicalDataValidator>();

            try
            {

                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var data = await _dbContext.Entity<MsMedicalItem>()
                    .Include(x => x.MappingMedicalItemConditions)
                    .Include(x => x.MedicalRecordMedicationDetails)
                    .Where(x => param.Ids.Contains(x.Id))
                    .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical Item not found");

                if (data.MappingMedicalItemConditions.Any() || data.MedicalRecordMedicationDetails.Any())
                {
                    throw new BadRequestException("Medical Item cannot be deleted because it is used in other data");
                }

                data.IsActive = false;

                _dbContext.Entity<MsMedicalItem>().Update(data);

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
