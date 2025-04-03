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
using BinusSchool.Student.FnStudent.MedicalSystem.Helper;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class GetGeneralMedicalInformationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IMachineDateTime _date;

        public GetGeneralMedicalInformationHandler(IStudentDbContext context, IMachineDateTime date)
        {
            _context = context;
            _date = date;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetGeneralMedicalInformationRequest>
                (nameof(GetGeneralMedicalInformationRequest.Mode),
                 nameof(GetGeneralMedicalInformationRequest.IdUser));

            var response = await GetGeneralMedicalInformation(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<GetGeneralMedicalInformationResponse> GetGeneralMedicalInformation(GetGeneralMedicalInformationRequest request)
        {
            var response = new GetGeneralMedicalInformationResponse();

            var IdUser = MedicalDecryptionValidation.ValidateDecryptionData(request.IdUser);

            var getMedicalGeneralInformation = _context.Entity<MsMedicalGeneralInformation>()
                .Where(a => a.IdUser == IdUser)
                .FirstOrDefault();

            if (getMedicalGeneralInformation == null)
                return response;

            response = new GetGeneralMedicalInformationResponse
            {
                HealthCondition = getMedicalGeneralInformation.HealthCondition,
                ApprovedMedicine = getMedicalGeneralInformation.ApprovalMedicine,
                Allergies = getMedicalGeneralInformation.AllergiesDetails,
                HealthNote = (request.Mode.ToLower() != "student" ? null : getMedicalGeneralInformation.HealthNotesFromParents),
                RegularMedication = (request.Mode.ToLower() != "student" ? null : getMedicalGeneralInformation.MedicationNotesFromParents),
            };

            return response;
        }
    }
}
