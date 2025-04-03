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
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalItem
{
    public class GetDetailMedicalItemHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetDetailMedicalItemHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<IdCollection>(nameof(IdCollection.Ids));

            var data = await _dbContext.Entity<MsMedicalItem>()
                .Include(x => x.DosageType)
                .Include(x => x.MedicalItemType)
                .Include(x => x.MedicineCategory)
                .Include(x => x.MedicineType)
                .Where(x => param.Ids.Contains(x.Id))
                .Select(x => new GetDetailMedicalItemResult
                {
                    Id = x.Id,
                    Description = x.MedicalItemName,
                    IsCommonDrug = x.IsCommonDrug,
                    DosageType = new CodeWithIdVm
                    {
                        Id = x.DosageType.Id,
                        Code = x.DosageType.DosageTypeMeasurement,
                        Description = x.DosageType.DosageTypeName
                    },
                    MedicalItemType = new ItemValueVm
                    {
                        Id = x.MedicalItemType.Id,
                        Description = x.MedicalItemType.MedicalItemTypeName
                    },
                    MedicineCategory = new ItemValueVm
                    {
                        Id = x.MedicineCategory.Id,
                        Description = x.MedicineCategory.MedicineCategoryName
                    },
                    MedicineType = new ItemValueVm
                    {
                        Id = x.MedicineType.Id,
                        Description = x.MedicineType.MedicineTypeName
                    }
                })
                .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical Item not found");

            return Request.CreateApiResult2(data as object);
        }
    }
}
