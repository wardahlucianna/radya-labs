﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalHospital;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalHospital
{
    public class GetDetailMedicalHospitalHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetDetailMedicalHospitalHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailMedicalHospitalRequest>(nameof(GetDetailMedicalHospitalRequest.IdHospital));

            var data = await _dbContext.Entity<MsMedicalHospital>()
                    .Where(x => x.Id == param.IdHospital)
                    .Select(x => new GetDetailMedicalHospitalResult
                    {
                        IdHospital = x.Id,
                        HospitalName = x.HospitalName,
                        HospitalAddress = x.HospitalAddress,
                        HospitalEmail = x.HospitalEmail,
                        HospitalPhoneNumber = x.HospitalPhoneNumber
                    })
                    .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }
    }
}
