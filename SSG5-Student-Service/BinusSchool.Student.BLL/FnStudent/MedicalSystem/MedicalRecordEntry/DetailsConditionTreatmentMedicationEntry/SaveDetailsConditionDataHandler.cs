using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.DetailsConditionTreatmentMedicationEntry;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Student.FnStudent.MedicalSystem.Helper;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NPOI.OpenXml4Net.OPC.Internal;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.DetailsConditionTreatmentMedicationEntry
{
    public class SaveDetailsConditionDataHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly IMachineDateTime _dateTime;
        private readonly IAttendanceAdministrationV2 _attendanceAdministrationV2;
        private readonly IPeriod _period;

        public SaveDetailsConditionDataHandler(IStudentDbContext dbContext, IMachineDateTime dateTime, IAttendanceAdministrationV2 attendanceAdministrationV2, IPeriod period)
        {
            _dateTime = dateTime;
            _dbContext = dbContext;
            _attendanceAdministrationV2 = attendanceAdministrationV2;
            _period = period;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveDetailsConditionDataRequest, SaveDetailsConditionDataValidator>();


            var idBinusian = MedicalDecryptionValidation.ValidateDecryptionData(param.Id);
            //var idBinusian = param.Id;

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                //Validate if param condition contains injury
                var getConditionData = await _dbContext.Entity<MsMedicalCondition>()
                    .Where(x => param.Data.IdConditions.Contains(x.Id) && x.IdSchool == param.IdSchool)
                    .AnyAsync(x => x.MedicalConditionName.Contains("injury"));
                if (getConditionData)
                {
                    if (param.Data.TeacherInCharge == null || param.Data.Location == null || param.Data.DetailsNotes == null)
                    {
                        throw new Exception("Teacher In Charge, Location, and Details Notes must be filled.");
                    }
                }

                //Check if condition and treatment is mapped correctly
                var treatmentIds = param.Data.IdTreatments;
                var medicationIds = param.Data.Medication.Select(m => m.IdMedicalItem);

                var treatmentExists = await _dbContext.Entity<TrMappingTreatmentCondition>()
                    .Where(x => param.Data.IdConditions.Contains(x.IdMedicalCondition) && treatmentIds.Contains(x.IdMedicalTreatment))
                    .Select(x => x.IdMedicalTreatment)
                    .Distinct()
                    .ToListAsync(CancellationToken);

                var medicationExists = await _dbContext.Entity<TrMappingMedicalItemCondition>()
                    .Where(x => param.Data.IdConditions.Contains(x.IdMedicalCondition) && medicationIds.Contains(x.IdMedicalItem))
                    .Select(x => x.IdMedicalItem)
                    .Distinct()
                    .ToListAsync(CancellationToken);

                if (treatmentExists.Count != treatmentIds.Count())
                {
                    throw new Exception("One or more treatments not found.");
                }

                if (medicationExists.Count != medicationIds.Count())
                {
                    throw new Exception("One or more medications not found.");
                }

                var paramCheckInDateWihtoutSecond = new DateTime(param.CheckInDate.Year, param.CheckInDate.Month, param.CheckInDate.Day, param.CheckInDate.Hour, param.CheckInDate.Minute, 0);

                var getRecord = await _dbContext.Entity<TrMedicalRecordEntry>()
                    .Where(x => x.IdUser == idBinusian && 
                    x.IdSchool == param.IdSchool)
                    .ToListAsync(CancellationToken);


                if (string.IsNullOrEmpty(param.Data.IdDetailsCondition))//Create
                {
                    var recordWithoutSeconds = getRecord
                        .Select(x => new
                        {
                            x.Id,
                            CheckInDateTimeWithoutSecond = new DateTime(x.CheckInDateTime.Year, x.CheckInDateTime.Month, x.CheckInDateTime.Day, x.CheckInDateTime.Hour, x.CheckInDateTime.Minute, 0)
                        })
                        .ToList();

                    //Check if there is data already
                    var isExistingData = recordWithoutSeconds.Where(x => x.CheckInDateTimeWithoutSecond == paramCheckInDateWihtoutSecond).Any();
                    if (isExistingData)
                    {
                        throw new Exception("Existing Data.");
                    }

                    var insertRecord = new TrMedicalRecordEntry()
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdUser = idBinusian,
                        CheckInDateTime = paramCheckInDateWihtoutSecond,
                        CheckOutDateTime = param.Data.CheckOutDate,
                        DismissedHome = param.Data.DismissedHome,
                        IdMedicalHospital = param.Data.IdHospital,
                        TeacherInCharge = param.Data.TeacherInCharge,
                        Location = param.Data.Location,
                        DetailsNotes = param.Data.DetailsNotes,
                        IdSchool = param.IdSchool
                    };

                    var insertConditionDetails = param.Data.IdConditions
                        .Select(x => new TrMedicalRecordConditionDetails
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMedicalRecordEntry = insertRecord.Id,
                            IdMedicalCondition = x
                        });

                    var insertTreatmentDetails = param.Data.IdTreatments
                        .Select(x => new TrMedicalRecordTreatmentDetails
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMedicalRecordEntry = insertRecord.Id,
                            IdMedicalTreatment = x
                        });

                    var insertMedicationDetails = param.Data.Medication
                        .Select(x => new TrMedicalRecordMedicationDetails
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMedicalRecordEntry = insertRecord.Id,
                            DosageAmount = x.DosageAmount,
                            IdMedicalItem = x.IdMedicalItem
                        });

                    _dbContext.Entity<TrMedicalRecordEntry>().Add(insertRecord);
                    _dbContext.Entity<TrMedicalRecordConditionDetails>().AddRange(insertConditionDetails);
                    _dbContext.Entity<TrMedicalRecordTreatmentDetails>().AddRange(insertTreatmentDetails);
                    _dbContext.Entity<TrMedicalRecordMedicationDetails>().AddRange(insertMedicationDetails);

                    if (param.Data.DismissedHome)
                    {
                        var addAttendance = await AddAttendance(idBinusian, param.IdSchool, paramCheckInDateWihtoutSecond);
                        //    if (!addAttendance)
                        //    {
                        //        throw new Exception("Failed to add attendance.");
                        //    }
                        //}
                    }
                }
                else
                {
                    var updateRecord = getRecord.Where(x => x.Id == param.Data.IdDetailsCondition).FirstOrDefault();

                    if (updateRecord == null)
                    {
                        throw new Exception("Data not found.");
                    }

                    var initialDismissedHome = updateRecord.DismissedHome;

                    updateRecord.CheckInDateTime = paramCheckInDateWihtoutSecond;
                    updateRecord.CheckOutDateTime = param.Data.CheckOutDate;
                    updateRecord.DismissedHome = param.Data.DismissedHome;
                    updateRecord.IdMedicalHospital = param.Data.IdHospital;
                    updateRecord.TeacherInCharge = param.Data.TeacherInCharge;
                    updateRecord.Location = param.Data.Location;
                    updateRecord.DetailsNotes = param.Data.DetailsNotes;

                    _dbContext.Entity<TrMedicalRecordEntry>().Update(updateRecord);

                    //Update Condition Details
                    var getConditionDetails = await _dbContext.Entity<TrMedicalRecordConditionDetails>()
                        .Where(x => x.IdMedicalRecordEntry == param.Data.IdDetailsCondition)
                        .ToListAsync(CancellationToken);
                    var insertConditionDetails = param.Data.IdConditions
                        .Where(x => !getConditionDetails.Select(x => x.IdMedicalCondition).Contains(x))
                        .Select(x => new TrMedicalRecordConditionDetails
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMedicalRecordEntry = updateRecord.Id,
                            IdMedicalCondition = x
                        });
                    var deleteConditionDetails = getConditionDetails
                        .Where(x => !param.Data.IdConditions.Contains(x.IdMedicalCondition))
                        .ToList();
                    deleteConditionDetails.ForEach(x => x.IsActive = false);

                    _dbContext.Entity<TrMedicalRecordConditionDetails>().AddRange(insertConditionDetails);
                    _dbContext.Entity<TrMedicalRecordConditionDetails>().UpdateRange(deleteConditionDetails);

                    //Update Treatment Details
                    var getTreatmentDetails = await _dbContext.Entity<TrMedicalRecordTreatmentDetails>()
                        .Where(x => x.IdMedicalRecordEntry == param.Data.IdDetailsCondition)
                        .ToListAsync(CancellationToken);
                    var insertTreatmentDetails = param.Data.IdTreatments
                        .Where(x => !getTreatmentDetails.Select(x => x.IdMedicalTreatment).Contains(x))
                        .Select(x => new TrMedicalRecordTreatmentDetails
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMedicalRecordEntry = updateRecord.Id,
                            IdMedicalTreatment = x
                        });
                    var deleteTreatmentDetails = getTreatmentDetails
                        .Where(x => !param.Data.IdTreatments.Contains(x.IdMedicalTreatment))
                        .ToList();
                    deleteTreatmentDetails.ForEach(x => x.IsActive = false);

                    _dbContext.Entity<TrMedicalRecordTreatmentDetails>().AddRange(insertTreatmentDetails);
                    _dbContext.Entity<TrMedicalRecordTreatmentDetails>().UpdateRange(deleteTreatmentDetails);

                    //Update Medication Details
                    var getMedicationDetails = await _dbContext.Entity<TrMedicalRecordMedicationDetails>()
                        .Where(x => x.IdMedicalRecordEntry == param.Data.IdDetailsCondition)
                        .ToListAsync(CancellationToken);
                    var updateMedicationDetails = getMedicationDetails
                        .Where(x => param.Data.Medication.Select(x => x.IdMedicalItem).Contains(x.IdMedicalItem))
                        .ToList();
                    var insertMedicationDetails = param.Data.Medication
                        .Where(x => !getMedicationDetails.Select(x => x.IdMedicalItem).Contains(x.IdMedicalItem))
                        .Select(x => new TrMedicalRecordMedicationDetails
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMedicalRecordEntry = updateRecord.Id,
                            DosageAmount = x.DosageAmount,
                            IdMedicalItem = x.IdMedicalItem
                        });
                    var deleteMedicationDetails = getMedicationDetails
                        .Where(x => !param.Data.Medication.Select(x => x.IdMedicalItem).Contains(x.IdMedicalItem))
                        .ToList();
                    deleteMedicationDetails.ForEach(x => x.IsActive = false);

                    foreach(var updateMedication in updateMedicationDetails)
                    {
                        var medicalItem = param.Data.Medication.Where(x => x.IdMedicalItem == updateMedication.IdMedicalItem).FirstOrDefault();
                        updateMedication.DosageAmount = medicalItem.DosageAmount;

                        _dbContext.Entity<TrMedicalRecordMedicationDetails>().Update(updateMedication);
                    }

                    _dbContext.Entity<TrMedicalRecordMedicationDetails>().AddRange(insertMedicationDetails);
                    _dbContext.Entity<TrMedicalRecordMedicationDetails>().UpdateRange(deleteMedicationDetails);

                    if (initialDismissedHome != param.Data.DismissedHome && param.Data.DismissedHome == true)
                    {
                        var addAttendance = await AddAttendance(idBinusian, param.IdSchool, paramCheckInDateWihtoutSecond);
                        //if (!addAttendance)
                        //{
                        //    throw new Exception("Failed to add attendance.");
                        //}
                    }

                }
                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }

            return Request.CreateApiResult2();
        }

        private async Task<bool> AddAttendance(string idBinusian, string idSchool, DateTime checkInDate)
        {
            var getCurrentAY = await _period.GetCurrenctAcademicYear(new Data.Model.School.FnPeriod.Period.CurrentAcademicYearRequest
            {
                IdSchool = idSchool,
            });

            var activeAY = getCurrentAY.Payload;

            var getStudent = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(a => a.Student)
                .Include(a => a.Homeroom.Grade.MsLevel.MsAcademicYear.MsSchool)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == activeAY.Id
                    && a.Semester == activeAY.Semester
                    && a.Homeroom.Semester == activeAY.Semester
                    && a.Student.Id == idBinusian)
                .FirstOrDefaultAsync(CancellationToken);

            if (getStudent != null)
            {
                var addAttendance = await _attendanceAdministrationV2.AddAttendanceAdministrationV2(new PostAttendanceAdministrationV2Request
                {
                    IdSchool = idSchool,
                    IsAllDay = true,
                    IdUser = AuthInfo.UserId,
                    Students = new List<PostAttendanceAdministrationStudentV2Request>
                            {
                                new PostAttendanceAdministrationStudentV2Request
                                {
                                    AbsencesFile = "",
                                    StartDate = checkInDate.Date,
                                    StartPeriod = checkInDate.TimeOfDay,
                                    EndDate = checkInDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                                    EndPeriod = new TimeSpan(23, 59, 59),
                                    IdStudent = idBinusian,
                                    IdGrade = getStudent.Homeroom.IdGrade,
                                    SendEmail = false,
                                    HomeRoom = string.Concat(getStudent.Homeroom.Grade.Description, getStudent.Homeroom.MsGradePathwayClassroom.Classroom.Code),
                                    IdAttendance = "19ef03fc-d256-4a27-9d12-6bdfee64a4f7",
                                    IncludeElective = false,
                                    NeedValidation = false,
                                    Reason = "Dismissed Home (Clinic)"
                                }
                            }
                });

                if (addAttendance.IsSuccess)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
