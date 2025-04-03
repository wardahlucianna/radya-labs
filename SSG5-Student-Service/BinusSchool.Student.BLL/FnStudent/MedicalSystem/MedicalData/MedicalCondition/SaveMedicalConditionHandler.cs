using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalCondition;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MedicalSystem.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalCondition
{
    public class SaveMedicalConditionHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        public SaveMedicalConditionHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveMedicalConditionRequest, SaveMedicalConditionValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                if (param.IdMedicalCondition == null)
                {
                    var saveData = new MsMedicalCondition
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdSchool = param.IdSchool,
                        MedicalConditionName = param.MedicalConditionName
                    };

                    _dbContext.Entity<MsMedicalCondition>().Add(saveData);

                    var saveMappingMedicalItemConditions = param.IdMedicalItem
                        .Select(x => new TrMappingMedicalItemCondition
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMedicalItem = x,
                            IdMedicalCondition = saveData.Id
                        });

                    var saveMappingMedicalTreatmentConditions = param.IdMedicalTreatment
                        .Select(x => new TrMappingTreatmentCondition
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMedicalTreatment = x,
                            IdMedicalCondition = saveData.Id
                        });

                    _dbContext.Entity<TrMappingMedicalItemCondition>().AddRange(saveMappingMedicalItemConditions);
                    _dbContext.Entity<TrMappingTreatmentCondition>().AddRange(saveMappingMedicalTreatmentConditions);
                }
                else
                {
                    var updateData = await _dbContext.Entity<MsMedicalCondition>()
                        .Where(x => x.Id == param.IdMedicalCondition && x.IdSchool == param.IdSchool)
                        .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical Condition not found");

                    updateData.MedicalConditionName = param.MedicalConditionName;
                    _dbContext.Entity<MsMedicalCondition>().Update(updateData);

                    var EditMappingMedicalItemConditions = await _dbContext.Entity<TrMappingMedicalItemCondition>()
                        .Where(x => x.IdMedicalCondition == param.IdMedicalCondition)
                        .ToListAsync(CancellationToken);

                    var EditMappingTreatmentConditions = await _dbContext.Entity<TrMappingTreatmentCondition>()
                        .Where(x => x.IdMedicalCondition == param.IdMedicalCondition)
                        .ToListAsync(CancellationToken);

                    // Medical Item
                    var mappingMedicalItemConditionsDelete = EditMappingMedicalItemConditions
                        .Where(x => !param.IdMedicalItem.Contains(x.IdMedicalItem))
                        .ToList();
                    var mappingMedicalItemConditionsInsert = param.IdMedicalItem
                        .Where(x => !EditMappingMedicalItemConditions.Any(y => y.IdMedicalItem == x))
                        .Select(x => new TrMappingMedicalItemCondition
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMedicalItem = x,
                            IdMedicalCondition = param.IdMedicalCondition
                        })
                        .ToList();

                    mappingMedicalItemConditionsDelete.ForEach(x => x.IsActive = false);
                    _dbContext.Entity<TrMappingMedicalItemCondition>().UpdateRange(mappingMedicalItemConditionsDelete);
                    _dbContext.Entity<TrMappingMedicalItemCondition>().AddRange(mappingMedicalItemConditionsInsert);

                    // Medical Treatment
                    var mappingTreatmentConditionsDelete = EditMappingTreatmentConditions
                        .Where(x => !param.IdMedicalTreatment.Contains(x.IdMedicalTreatment))
                        .ToList();
                    var mappingTreatmentConditionsInsert = param.IdMedicalTreatment
                        .Where(x => !EditMappingTreatmentConditions.Any(y => y.IdMedicalTreatment == x))
                        .Select(x => new TrMappingTreatmentCondition
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMedicalTreatment = x,
                            IdMedicalCondition = param.IdMedicalCondition
                        })
                        .ToList();

                    mappingTreatmentConditionsDelete.ForEach(x => x.IsActive = false);
                    _dbContext.Entity<TrMappingTreatmentCondition>().UpdateRange(mappingTreatmentConditionsDelete);
                    _dbContext.Entity<TrMappingTreatmentCondition>().AddRange(mappingTreatmentConditionsInsert);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);
            }
            catch (Exception ex)
            {
                await _transaction.RollbackAsync(CancellationToken);
                throw ex;
            }

            return Request.CreateApiResult2();
        }

    }
}
