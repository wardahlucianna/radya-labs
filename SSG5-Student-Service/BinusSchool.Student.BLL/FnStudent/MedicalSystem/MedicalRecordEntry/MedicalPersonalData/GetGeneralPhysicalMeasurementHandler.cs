using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MedicalSystem.Helper;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class GetGeneralPhysicalMeasurementHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IMachineDateTime _date;

        public GetGeneralPhysicalMeasurementHandler(IStudentDbContext context, IMachineDateTime date)
        {
            _context = context;
            _date = date;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetGeneralPhysicalMeasurementRequest>(
                nameof(GetGeneralPhysicalMeasurementRequest.Id),
                nameof(GetGeneralPhysicalMeasurementRequest.Mode));

            var response = await GetGeneralPhysicalMeasurement(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<List<GetGeneralPhysicalMeasurementResponse>> GetGeneralPhysicalMeasurement(GetGeneralPhysicalMeasurementRequest request)
        {
            var response = new List<GetGeneralPhysicalMeasurementResponse>();

            DateTime date = _date.ServerTime;
            var IdUser = MedicalDecryptionValidation.ValidateDecryptionData(request.Id);

            var medicalPhysicalMeasurement = await _context.Entity<TrMedicalPhysicalMeasurement>()
                .Where(a => a.IdUser == IdUser)
                .ToListAsync(CancellationToken);

            if (!medicalPhysicalMeasurement.Any())
                return response;

            var user = await _context.Entity<MsUser>()
                .ToListAsync(CancellationToken);

            response = medicalPhysicalMeasurement
                .Select(a => new GetGeneralPhysicalMeasurementResponse
                {
                    Weight = a.Weight,
                    Height = a.Height,
                    BloodPressure = a.BloodPressure,
                    BodyTemperature = a.BodyTemperature,
                    HeartRate = a.HeartRate,
                    Saturation = a.Saturation,
                    RespiratoryRate = a.RespiratoryRate,
                    MeasurementDate = a.MeasurementDate,
                    MeasurementPIC = a.MeasurementPIC,
                    InputBy = user.Where(b => b.Id == a.UserIn).Select(b => b.DisplayName).FirstOrDefault(),
                })
                .OrderByDescending(a => a.MeasurementDate)
                .ToList();

            return response;
        }
    }
}
