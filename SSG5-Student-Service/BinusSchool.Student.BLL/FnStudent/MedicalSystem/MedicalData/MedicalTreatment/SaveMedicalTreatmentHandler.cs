using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalTreatment;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MedicalSystem.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalTreatment
{
    public class SaveMedicalTreatmentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public SaveMedicalTreatmentHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveMedicalTreatmentRequest, SaveMedicalTreatmentValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                if (param.IdMedicalTreatment == null)
                {
                    var insertData = new MsMedicalTreatment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdSchool = param.IdSchool,
                        MedicalTreatmentName = param.MedicalTreatmentName
                    };

                    _dbContext.Entity<MsMedicalTreatment>().Add(insertData);
                }
                else
                {
                    var updateData = await _dbContext.Entity<MsMedicalTreatment>()
                        .Where(x => x.Id == param.IdMedicalTreatment)
                        .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical Treatment not found");

                    updateData.MedicalTreatmentName = param.MedicalTreatmentName;

                    _dbContext.Entity<MsMedicalTreatment>().Update(updateData);
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

            return Request.CreateApiResult2();
        }
    }
}
