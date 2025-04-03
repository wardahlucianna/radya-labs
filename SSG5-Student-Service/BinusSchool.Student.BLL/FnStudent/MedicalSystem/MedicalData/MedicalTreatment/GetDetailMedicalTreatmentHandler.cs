using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalTreatment
{
    public class GetDetailMedicalTreatmentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetDetailMedicalTreatmentHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<IdCollection>(nameof(IdCollection.Ids));

            var getData = await _dbContext.Entity<MsMedicalTreatment>()
                .Where(x => param.Ids.Contains(x.Id))
                .Select(x => new ItemValueVm
                {
                    Id = x.Id,
                    Description = x.MedicalTreatmentName
                })
                .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical Treatment not found");

            return Request.CreateApiResult2(getData as object);
        }
    }
}
