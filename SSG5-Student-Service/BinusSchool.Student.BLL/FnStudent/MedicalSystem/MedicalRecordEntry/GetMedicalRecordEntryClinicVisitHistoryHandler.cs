using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MedicalSystem.Helper;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry
{
    public class GetMedicalRecordEntryClinicVisitHistoryHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IMachineDateTime _time;

        public GetMedicalRecordEntryClinicVisitHistoryHandler(IStudentDbContext context, IMachineDateTime time)
        {
            _context = context;
            _time = time;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetMedicalRecordEntryClinicVisitHistoryRequest>
                (nameof(GetMedicalRecordEntryClinicVisitHistoryRequest.IdSchool),
                 nameof(GetMedicalRecordEntryClinicVisitHistoryRequest.IdUser),
                 nameof(GetMedicalRecordEntryClinicVisitHistoryRequest.Type));

            var response = await GetMedicalRecordEntryClinicVisitHistory(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<List<GetMedicalRecordEntryClinicVisitHistoryResponse>> GetMedicalRecordEntryClinicVisitHistory(GetMedicalRecordEntryClinicVisitHistoryRequest request)
        {
            var response = new List<GetMedicalRecordEntryClinicVisitHistoryResponse>();

            DateTime date = _time.ServerTime;

            var idUser = MedicalDecryptionValidation.ValidateDecryptionData(request.IdUser);

            var medicalRecord = _context.Entity<TrMedicalRecordEntry>()
                .Include(a => a.MedicalRecordConditionDetails)
                    .ThenInclude(b => b.MedicalCondition)
                .Include(a => a.MedicalRecordTreatmentDetails)
                    .ThenInclude(b => b.MedicalTreatment)
                .Include(a => a.MedicalRecordMedicationDetails)
                    .ThenInclude(b => b.MedicalItem)
                .Where(a => (request.Type.ToLower() == "accumulative" ? true : a.CheckInDateTime.Month == date.Month)
                    && a.IdUser == idUser)
                .ToList();

            var insertResponse = medicalRecord
                .Select(a => new GetMedicalRecordEntryClinicVisitHistoryResponse
                {
                    IdMedicalRecordEntry = a.Id,
                    VisitDateTime = a.CheckInDateTime,
                    Conditions = a.MedicalRecordConditionDetails
                        .Select(b => b.MedicalCondition.MedicalConditionName)
                        .ToList(),
                    Treatments = a.MedicalRecordTreatmentDetails
                        .Select(b => b.MedicalTreatment.MedicalTreatmentName)
                        .ToList(),
                    Medications = a.MedicalRecordMedicationDetails
                        .Select(b => b.MedicalItem.MedicalItemName)
                        .ToList(),
                    Notes = a.DetailsNotes,
                    PIC = a.TeacherInCharge
                }).ToList();

            response.AddRange(insertResponse);

            return response;
        }
    }
}
