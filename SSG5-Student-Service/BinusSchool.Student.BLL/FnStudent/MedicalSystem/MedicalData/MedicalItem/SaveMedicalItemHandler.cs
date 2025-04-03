using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalItem;
using BinusSchool.Persistence.Abstractions;
using BinusSchool.Persistence.StudentDb;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.MedicalSystem.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalItem
{
    public class SaveMedicalItemHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveMedicalItemHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveMedicalItemRequest, SaveMedicalItemValidator>();

            var checkMedicalItemType = await _dbContext.Entity<LtMedicalItemType>()
                .Where(x => x.Id == param.IdMedicalItemType)
                .FirstOrDefaultAsync(CancellationToken);


            if (checkMedicalItemType == null)
            {
                throw new NotFoundException("Medical Category not found");
            }

            if (checkMedicalItemType.MedicalItemTypeName.Contains("Medicine"))
            {
                if (param.IdMedicineType == null)
                {
                    throw new BadRequestException("Medicine Type is required");
                }

                if (param.IdMedicineCategory == null)
                {
                    throw new BadRequestException("Medicine Category is required");
                }
            }

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                if (param.IdMedicalItem == null)
                {
                    var createData = new MsMedicalItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        MedicalItemName = param.MedicalItemName,
                        IdMedicalItemType = param.IdMedicalItemType,
                        IsCommonDrug = param.IsCommonDrug,
                        IdMedicineType = param.IdMedicineType,
                        IdMedicineCategory = param.IdMedicineCategory,
                        IdDosageType = param.IdDosageType,
                        IdSchool = param.IdSchool
                    };

                    _dbContext.Entity<MsMedicalItem>().Add(createData);
                }
                else
                {
                    var updateData = await _dbContext.Entity<MsMedicalItem>()
                        .Where(x => x.Id == param.IdMedicalItem && x.IdSchool == param.IdSchool)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (updateData == null)
                    {
                        throw new NotFoundException("Medical Item not found");
                    }

                    updateData.MedicalItemName = param.MedicalItemName;
                    updateData.IdMedicalItemType = param.IdMedicalItemType;
                    updateData.IsCommonDrug = param.IsCommonDrug;
                    updateData.IdMedicineType = param.IdMedicineType;
                    updateData.IdMedicineCategory = param.IdMedicineCategory;
                    updateData.IdDosageType = param.IdDosageType;

                    _dbContext.Entity<MsMedicalItem>().Update(updateData);
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
