using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalDoctor;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalDoctor
{
    public class GetListMedicalDoctorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListMedicalDoctorHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<MedicalDataSchoolRequest>(nameof(MedicalDataSchoolRequest.IdSchool));

            var data = await _dbContext.Entity<MsMedicalDoctor>()
                .Where(x => x.IdSchool == param.IdSchool)
                .Select(x => new GetListMedicalDoctorResult
                {
                    IdMedicalDoctor = x.Id,
                    DoctorName = x.DoctorName,
                    DoctorAddress = x.DoctorAddress,
                    DoctorPhoneNumber = x.DoctorPhoneNumber,
                    DoctorEmail = x.DoctorEmail,
                    CanDelete = true
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);

        }
    }
}
