using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalItem;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalItem
{
    public class GetListMedicalItemHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListMedicalItemHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<MedicalDataSchoolRequest>(nameof(MedicalDataSchoolRequest.IdSchool));

            var data = await _dbContext.Entity<MsMedicalItem>()
                .Include(x => x.DosageType)
                .Include(x => x.MedicalItemType)
                .Where(x => x.IdSchool == param.IdSchool)
                .Select(x => new GetListMedicalItemResult
                {
                    Id = x.Id,
                    Description = x.MedicalItemName,
                    MedicalItemType = x.MedicalItemType.MedicalItemTypeName,
                    IsCommonDrug = x.IsCommonDrug,
                    DosageType = x.DosageType.DosageTypeName,
                    CanDelete = !(x.MappingMedicalItemConditions.Any() || x.MedicalRecordMedicationDetails.Any())
                })
                .ToListAsync(CancellationToken);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = data.Select(x => new ItemValueVm(x.Id, x.Description)).ToList();
            }
            else
            {
                items = data;
            }

            return Request.CreateApiResult2(items as object);
        }
    }
}
