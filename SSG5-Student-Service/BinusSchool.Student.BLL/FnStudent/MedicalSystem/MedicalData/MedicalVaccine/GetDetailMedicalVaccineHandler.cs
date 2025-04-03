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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalVaccine
{
    public class GetDetailMedicalVaccineHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetDetailMedicalVaccineHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<IdCollection>(nameof(IdCollection.Ids));

            var getData = await _dbContext.Entity<MsMedicalVaccine>()
                .Where(x => param.Ids.Contains(x.Id))
                .Select(x => new GetDetailMedicalVaccineResult
                {
                    Id = x.Id,
                    Description = x.MedicalVaccineName,
                    DosageType = new ItemValueVm
                    {
                        Id = x.IdDosageType,
                        Description = x.DosageType.DosageTypeName
                    },
                    DosageAmount = x.DosageAmount,
                })
                .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical vaccine not found");

            return Request.CreateApiResult2(getData as object);
        }
    }
}
