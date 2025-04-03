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

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalTreatment
{
    public class DeleteMedicalTreatmentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteMedicalTreatmentHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<IdCollection, DeleteMedicalDataValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getData = await _dbContext.Entity<MsMedicalTreatment>()
                    .Include(x => x.MappingTreatmentConditions)
                    .Include(x => x.MedicalRecordTreatmentDetails)
                    .Where(x => param.Ids.Contains(x.Id))
                    .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical Treatment not found");

                if(getData.MappingTreatmentConditions.Any() || getData.MedicalRecordTreatmentDetails.Any())
                {
                    throw new BadRequestException("Medical Treatment cannot be deleted because it is used in other data");
                }

                getData.IsActive = false;

                _dbContext.Entity<MsMedicalTreatment>().Update(getData);

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
