using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalDoctor;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalDoctor
{
    public class GetDetailMedicalDoctorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public GetDetailMedicalDoctorHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<IdCollection>(nameof(IdCollection));

            var data = await _dbContext.Entity<MsMedicalDoctor>()
                .Include(x => x.MedicalHospital)
                .Where(x => param.Ids.Contains(x.Id))
                .Select(x => new GetDetailMedicalDoctorResult
                {
                    IdMedicalDoctor = x.Id,
                    DoctorName = x.DoctorName,
                    DoctorAddress = x.DoctorAddress,
                    DoctorPhoneNumber = x.DoctorPhoneNumber,
                    DoctorEmail = x.DoctorEmail,
                    Hospital = new GetDetailMedicalDoctorResult_Hospital
                    {
                        IdMedicalHospital = x.IdMedicalHospital,
                        HospitalName = x.MedicalHospital.HospitalName,
                        HospitalAddress = x.MedicalHospital.HospitalAddress,
                        HospitalPhoneNumber = x.MedicalHospital.HospitalPhoneNumber,
                        HospitalEmail = x.MedicalHospital.HospitalEmail
                    }
                })
                .FirstOrDefaultAsync(CancellationToken) ?? throw new NotFoundException("Medical doctor not found");

            return Request.CreateApiResult2(data as object);
        }
    }
}
