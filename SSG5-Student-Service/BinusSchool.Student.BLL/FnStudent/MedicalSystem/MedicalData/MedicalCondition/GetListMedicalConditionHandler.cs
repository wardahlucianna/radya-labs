using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalCondition;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalCondition
{
    public class GetListMedicalConditionHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListMedicalConditionHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<MedicalDataSchoolRequest>(nameof(MedicalDataSchoolRequest.IdSchool));

            var data = await _dbContext.Entity<MsMedicalCondition>()
                .Include(x => x.MappingMedicalItemConditions).ThenInclude(x => x.MedicalItem)
                .Include(x => x.MappingTreatmentConditions).ThenInclude(x => x.MedicalTreatment)
                .Include(x => x.MedicalRecordConditionDetails)
                .Where(x => x.IdSchool == param.IdSchool)
                .Select(x => new GetListMedicalConditionResult
                {
                    Id = x.Id,
                    Description = x.MedicalConditionName,
                    MedicatedWith = x.MappingMedicalItemConditions.Select(y => y.MedicalItem.MedicalItemName).ToList(),
                    TreatedWith = x.MappingTreatmentConditions.Select(y => y.MedicalTreatment.MedicalTreatmentName).ToList(),
                    CanDelete = !(x.MedicalRecordConditionDetails.Any())
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }
    }
}
