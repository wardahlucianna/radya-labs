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

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalCondition
{
    public class DeleteMedicalConditionHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteMedicalConditionHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<IdCollection, DeleteMedicalDataValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var deleteData = await _dbContext.Entity<MsMedicalCondition>()
                    .Include(x => x.MedicalRecordConditionDetails)
                    .Where(x => param.Ids.Contains(x.Id))
                    .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical condition not found");

                if (deleteData.MedicalRecordConditionDetails.Any())
                {
                    throw new BadRequestException("Medical condition cannot be deleted because it is used in other data");
                }

                var deleteMappingMedicalItem = await _dbContext.Entity<TrMappingMedicalItemCondition>()
                    .Where(x => x.IdMedicalCondition == deleteData.Id)
                    .ToListAsync(CancellationToken);

                var deleteMappingMedicalTreatment = await _dbContext.Entity<TrMappingTreatmentCondition>()
                    .Where(x => x.IdMedicalCondition == deleteData.Id)
                    .ToListAsync(CancellationToken);

                deleteData.IsActive = false;
                deleteMappingMedicalItem.ForEach(x => x.IsActive = false);
                deleteMappingMedicalTreatment.ForEach(x => x.IsActive = false);

                _dbContext.Entity<MsMedicalCondition>().Update(deleteData);
                _dbContext.Entity<TrMappingMedicalItemCondition>().UpdateRange(deleteMappingMedicalItem);
                _dbContext.Entity<TrMappingTreatmentCondition>().UpdateRange(deleteMappingMedicalTreatment);

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
