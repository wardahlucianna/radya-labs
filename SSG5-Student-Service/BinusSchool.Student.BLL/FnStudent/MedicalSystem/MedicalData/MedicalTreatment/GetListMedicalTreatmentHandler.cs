using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalTreatment;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalTreatment
{
    public class GetListMedicalTreatmentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListMedicalTreatmentHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<MedicalDataSchoolRequest>(nameof(MedicalDataSchoolRequest.IdSchool));

            var data = await _dbContext.Entity<MsMedicalTreatment>()
                .Where(x => x.IdSchool == param.IdSchool)
                .Select(x => new GetListMedicalTreatmentResult
                {
                    Id = x.Id,
                    Description = x.MedicalTreatmentName,
                    CanDelete = !(x.MappingTreatmentConditions.Any() || x.MedicalRecordTreatmentDetails.Any())
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
