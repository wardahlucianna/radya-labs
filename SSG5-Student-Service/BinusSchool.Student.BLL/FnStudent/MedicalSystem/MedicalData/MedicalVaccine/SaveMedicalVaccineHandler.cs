using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalVaccine;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MedicalSystem.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalVaccine
{
    public class SaveMedicalVaccineHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveMedicalVaccineHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveMedicalVaccineRequest, SaveMedicalVaccineValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                if(param.IdMedicalVaccine == null)
                {
                    var insertData = new MsMedicalVaccine
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdDosageType = param.IdDosageType,
                        MedicalVaccineName = param.MedicalVaccineName,
                        DosageAmount = param.DosageAmount,
                        IdSchool = param.IdSchool
                    };

                    _dbContext.Entity<MsMedicalVaccine>().Add(insertData);
                }
                else
                {
                    var updateData = await _dbContext.Entity<MsMedicalVaccine>()
                        .Where(x => x.Id == param.IdMedicalVaccine)
                        .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical Vaccine not found");

                    updateData.IdDosageType = param.IdDosageType;
                    updateData.MedicalVaccineName = param.MedicalVaccineName;
                    updateData.DosageAmount = param.DosageAmount;

                    _dbContext.Entity<MsMedicalVaccine>().Update(updateData);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);
            }
            catch(Exception ex)
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
