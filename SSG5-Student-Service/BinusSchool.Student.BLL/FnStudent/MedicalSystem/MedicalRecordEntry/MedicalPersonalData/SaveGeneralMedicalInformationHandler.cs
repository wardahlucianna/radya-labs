using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MedicalSystem.Helper;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.Validator;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class SaveGeneralMedicalInformationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private IDbContextTransaction _transaction;

        public SaveGeneralMedicalInformationHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<SaveGeneralMedicalInformationRequest, SaveGeneralMedicalInformationValidator>();

            await SaveGeneralMedicalInformation(request);

            return Request.CreateApiResult2(code: HttpStatusCode.Created);
        }

        public async Task SaveGeneralMedicalInformation(SaveGeneralMedicalInformationRequest request)
        {
            using (_transaction = await _context.BeginTransactionAsync(CancellationToken, IsolationLevel.Serializable))
            {
                try
                {
                    var IdUser = MedicalDecryptionValidation.ValidateDecryptionData(request.Id);

                    if (request.Mode.ToLower() != "student" &&
                        (!string.IsNullOrEmpty(request.HealthNote) || !string.IsNullOrEmpty(request.RegularMedication)))
                    {
                        throw new BadRequestException("Staff/Other no need to fill Student Health or Student Regular Medication");
                    }

                    bool checkDataExist = _context.Entity<MsMedicalGeneralInformation>()
                        .Where(a => a.IdUser == IdUser)
                        .Any();

                    if (!checkDataExist) // do create
                    {
                        var insertData = new MsMedicalGeneralInformation()
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdUser = IdUser,
                            HealthCondition = request.HealthCondition,
                            ApprovalMedicine = request.ApprovedMedicine,
                            AllergiesDetails = request.Allergies,
                            HealthNotesFromParents = request.HealthNote,
                            MedicationNotesFromParents = request.RegularMedication
                        };

                        _context.Entity<MsMedicalGeneralInformation>().Add(insertData);
                    }
                    else // do update
                    {
                        var medicalGeneralInformation = _context.Entity<MsMedicalGeneralInformation>()
                            .Where(a => a.IdUser == IdUser)
                            .FirstOrDefault();

                        medicalGeneralInformation.HealthCondition = request.HealthCondition;
                        medicalGeneralInformation.ApprovalMedicine = request.ApprovedMedicine;
                        medicalGeneralInformation.AllergiesDetails = request.Allergies;
                        medicalGeneralInformation.HealthNotesFromParents = request.HealthNote;
                        medicalGeneralInformation.MedicationNotesFromParents = request.RegularMedication;

                        _context.Entity<MsMedicalGeneralInformation>().Update(medicalGeneralInformation);
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
