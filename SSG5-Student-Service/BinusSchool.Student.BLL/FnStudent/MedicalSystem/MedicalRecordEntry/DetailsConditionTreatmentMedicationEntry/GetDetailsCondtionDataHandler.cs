using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.DetailsConditionTreatmentMedicationEntry;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MedicalSystem.Helper;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.DetailsConditionTreatmentMedicationEntry
{
    public class GetDetailsCondtionDataHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetDetailsCondtionDataHandler(IStudentDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailsConditionDataRequest>(
                nameof(GetDetailsConditionDataRequest.Id),
                nameof(GetDetailsConditionDataRequest.IdSchool));

            var idBinusian = MedicalDecryptionValidation.ValidateDecryptionData(param.Id);
            //var idBinusian = param.Id;

            var getMedicalRecordEntry = await _dbContext.Entity<TrMedicalRecordEntry>()
                .Include(x => x.MedicalRecordConditionDetails).ThenInclude(x => x.MedicalCondition)
                .Include(x => x.MedicalRecordTreatmentDetails).ThenInclude(x => x.MedicalTreatment)
                .Include(x => x.MedicalRecordMedicationDetails).ThenInclude(x => x.MedicalItem).ThenInclude(x => x.MedicalItemType)
                .Include(x => x.MedicalRecordMedicationDetails).ThenInclude(x => x.MedicalItem).ThenInclude(x => x.DosageType)
                .Include(x => x.MedicalHospital)
                .Where(x => x.IdUser == idBinusian &&
                            x.CheckInDateTime == (param.CheckInDate == null ? x.CheckInDateTime : param.CheckInDate) &&
                            x.IdSchool == param.IdSchool)
                .ToListAsync(CancellationToken);

            var res = new GetDetailsCondtionDataResult();

            var filteredMedicalRecordEntry = new TrMedicalRecordEntry();
            if (param.CheckInDate != null)
            {
                filteredMedicalRecordEntry = getMedicalRecordEntry.Where(x => x.CheckInDateTime == param.CheckInDate).FirstOrDefault();
            }
            else
            {
                filteredMedicalRecordEntry = getMedicalRecordEntry?.Where(x => x.CheckInDateTime.Date == _dateTime.ServerTime.Date).OrderByDescending(x => x.CheckInDateTime).FirstOrDefault();
            }

            if (filteredMedicalRecordEntry != null)
            {
                res.CheckInDate = filteredMedicalRecordEntry.CheckInDateTime;
                var res_data = new GetDetailsCondtionDataResult_Data
                {
                    IdDetailsCondition = filteredMedicalRecordEntry.Id,
                    CheckOutDate = filteredMedicalRecordEntry.CheckOutDateTime,
                    Conditions = filteredMedicalRecordEntry.MedicalRecordConditionDetails?
                                    .Select(y => new ItemValueVm
                                    {
                                        Id = y.MedicalCondition.Id,
                                        Description = y.MedicalCondition.MedicalConditionName
                                    }).ToList(),
                    Treatments = filteredMedicalRecordEntry.MedicalRecordTreatmentDetails?
                                    .Select(y => new ItemValueVm
                                    {
                                        Id = y.MedicalTreatment.Id,
                                        Description = y.MedicalTreatment.MedicalTreatmentName
                                    }).ToList(),
                    MedicalItems = filteredMedicalRecordEntry.MedicalRecordMedicationDetails?
                                    .Select(y => new GetDetailsCondtionDataResult_MedicalItems
                                    {
                                        IdMedicalItem = y.IdMedicalItem,
                                        Description = y.MedicalItem.MedicalItemName,
                                        MedicalItemType = y.MedicalItem.MedicalItemType.MedicalItemTypeName,
                                        DosageType = y.MedicalItem.DosageType.DosageTypeName,
                                        DosageAmount = y.DosageAmount
                                    }).ToList(),
                    TeacherInCharge = filteredMedicalRecordEntry.TeacherInCharge,
                    Location = filteredMedicalRecordEntry.Location,
                    Hospital = filteredMedicalRecordEntry.IdMedicalHospital == null ? null : new ItemValueVm
                    {
                        Id = filteredMedicalRecordEntry.IdMedicalHospital,
                        Description = filteredMedicalRecordEntry.MedicalHospital?.HospitalName
                    },
                    DismissedHome = filteredMedicalRecordEntry.DismissedHome,
                    DetailsNotes = filteredMedicalRecordEntry.DetailsNotes
                };
                res.Data = res_data;
            }
            else
            {
                var checkInData = param.CheckInDate ?? _dateTime.ServerTime;
                res.CheckInDate = DateTime.SpecifyKind(checkInData, DateTimeKind.Unspecified);
            }

            return Request.CreateApiResult2(res as object);
        }
    }
}
