using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MedicalSystem.Helper;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class SaveGeneralPhysicalMeasurementHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private IDbContextTransaction _transaction;

        public SaveGeneralPhysicalMeasurementHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<SaveGeneralPhysicalMeasurementRequest, SaveGeneralPhysicalMeasurementValidator>();

            await SaveGeneralPhysicalMeasurement(request);

            return Request.CreateApiResult2(code: System.Net.HttpStatusCode.Created);
        }

        public async Task SaveGeneralPhysicalMeasurement(SaveGeneralPhysicalMeasurementRequest request)
        {
            using (_transaction =  await _context.BeginTransactionAsync(CancellationToken))
            {
                try
                {
                    var IdUser = MedicalDecryptionValidation.ValidateDecryptionData(request.Id);

                    if (string.IsNullOrEmpty(request.IdMedicalPhysicalMeasurement)) //create
                    {
                        var insertData = new TrMedicalPhysicalMeasurement()
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdUser = IdUser,
                            Height = request.Height,
                            Weight = request.Weight,
                            BloodPressure = request.BloodPressure,
                            BodyTemperature = request.BodyTemperature,
                            HeartRate = request.HeartRate,
                            Saturation = request.Saturation,
                            RespiratoryRate = request.RespiratoryRate,
                            MeasurementDate = request.MeasurementDate,
                            MeasurementPIC = request.MeasurementPIC,
                        };

                        _context.Entity<TrMedicalPhysicalMeasurement>().Add(insertData);
                    }
                    else //update
                    {
                        var medicalPhysicalMeasurement = await _context.Entity<TrMedicalPhysicalMeasurement>()
                            .Where(a => a.Id == request.IdMedicalPhysicalMeasurement
                                && a.IdUser == IdUser)
                            .FirstOrDefaultAsync(CancellationToken);

                        medicalPhysicalMeasurement.Height = request.Height;
                        medicalPhysicalMeasurement.Weight = request.Weight;
                        medicalPhysicalMeasurement.BloodPressure = request.BloodPressure;
                        medicalPhysicalMeasurement.BodyTemperature = request.BodyTemperature;
                        medicalPhysicalMeasurement.HeartRate = request.HeartRate;
                        medicalPhysicalMeasurement.Saturation = request.Saturation;
                        medicalPhysicalMeasurement.RespiratoryRate += request.RespiratoryRate;
                        medicalPhysicalMeasurement.MeasurementDate = request.MeasurementDate;
                        medicalPhysicalMeasurement.MeasurementPIC = request.MeasurementPIC;

                        _context.Entity<TrMedicalPhysicalMeasurement>().Update(medicalPhysicalMeasurement);
                    }

                    await _context.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);
                }
                catch (Exception ex)
                {
                    _transaction?.Rollback();

                    throw new Exception(ex.Message.ToString());
                }
            }
        }
    }
}
