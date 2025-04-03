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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalCondition
{
    public class GetDetailMedicalConditionHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetDetailMedicalConditionHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<IdCollection>(nameof(IdCollection.Ids));
            
            var getData = await _dbContext.Entity<MsMedicalCondition>()
                .Include(x => x.MappingMedicalItemConditions).ThenInclude(x => x.MedicalItem)
                .Include(x => x.MappingTreatmentConditions).ThenInclude(x => x.MedicalTreatment)
                .Where(x => param.Ids.Contains(x.Id))
                .Select(x => new GetDetailMedicalConditionResult
                {
                    Id = x.Id,
                    Description = x.MedicalConditionName,
                    MedicatedWith = x.MappingMedicalItemConditions.Select(y => y.IdMedicalItem).ToList(),
                    TreatedWith = x.MappingTreatmentConditions.Select(y => y.IdMedicalTreatment).ToList()
                })
                .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical condition not found");

            return Request.CreateApiResult2(getData as object);
        }
    }
}
