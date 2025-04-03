using System;
using System.Collections.Generic;
using System.Data;
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
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalTappingSystem;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Student.FnStudent.MedicalSystem.Helper;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalTappingSystem
{
    public class UpdateMedicalTappingSystemPatientStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IMachineDateTime _time;
        private readonly IPeriod _period;
        private readonly IConfiguration _configuration;
        private IDbContextTransaction _transaction;

        public UpdateMedicalTappingSystemPatientStatusHandler(IStudentDbContext context, IMachineDateTime time, IPeriod period, IConfiguration configuration)
        {
            _context = context;
            _time = time;
            _period = period;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.GetBody<UpdateMedicalTappingSystemPatientStatusRequest>();

            var response = await UpdateMedicalTappingSystemPatientStatus(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<UpdateMedicalTappingSystemPatientStatusResponse> UpdateMedicalTappingSystemPatientStatus(UpdateMedicalTappingSystemPatientStatusRequest request)
        {
            var response = new UpdateMedicalTappingSystemPatientStatusResponse();
            DateTime time = _time.ServerTime;

            var timeWithoutSecond = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);

            var idBinusian = MedicalDecryptionValidation.ValidateDecryptionData(request.IdBinusian);

            var validateCheckIn = _context.Entity<TrMedicalRecordEntry>()
                .Where(a => a.IdUser == idBinusian
                    && a.CheckInDateTime.Date == time.Date)
                .OrderByDescending(a => a.CheckInDateTime)
                .FirstOrDefault();

            if (validateCheckIn != null)
            {
                if (request.Status == 1 && validateCheckIn.CheckOutDateTime == null)
                    throw new BadRequestException($"Patient have do check in before in {validateCheckIn.CheckInDateTime.ToString("dd MMMM yyyy, hh:mm")}");

                if (request.Status == 0 && (validateCheckIn.CheckInDateTime != null && validateCheckIn.CheckOutDateTime != null))
                    throw new BadRequestException($"Patient have do check out before in {validateCheckIn.CheckOutDateTime.Value.ToString("dd MMMM yyyy, hh:mm")}");

                var checkInWithoutSecond = new DateTime(validateCheckIn.CheckInDateTime.Year,
                    validateCheckIn.CheckInDateTime.Month,
                    validateCheckIn.CheckInDateTime.Day,
                    validateCheckIn.CheckInDateTime.Hour,
                    validateCheckIn.CheckInDateTime.Minute, 0);

                if (checkInWithoutSecond == timeWithoutSecond)
                    throw new BadRequestException($"Patient cannont do check in at the same minute(s)");
            }

            using (_transaction = await _context.BeginTransactionAsync(CancellationToken, IsolationLevel.Serializable))
            {
                try
                {
                    if (request.Status == 1)
                    {
                        var insertMedicalRecord = new TrMedicalRecordEntry()
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdUser = idBinusian,
                            CheckInDateTime = timeWithoutSecond,
                            CheckOutDateTime = null,
                            DismissedHome = false,
                            IdSchool = request.IdSchool
                        };

                        _context.Entity<TrMedicalRecordEntry>().Add(insertMedicalRecord);
                    }
                    else
                    {
                        var updateMedicalRecord = _context.Entity<TrMedicalRecordEntry>()
                            .Where(a => a.IdUser == idBinusian
                                && a.CheckInDateTime.Date == time.Date)
                            .OrderByDescending(a => a.CheckInDateTime)
                            .FirstOrDefault();

                        updateMedicalRecord.CheckOutDateTime = timeWithoutSecond;

                        _context.Entity<TrMedicalRecordEntry>().Update(updateMedicalRecord);
                    }

                    await _context.SaveChangesAsync();
                    await _transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _transaction?.Rollback();

                    throw new Exception(ex.Message.ToString());
                }
            }

            var getCurrentAY = await _period.GetCurrenctAcademicYear(new CurrentAcademicYearRequest
            {
                IdSchool = request.IdSchool,
            });

            var activeAY = getCurrentAY.Payload;

            var getMedicalRecord = _context.Entity<TrMedicalRecordEntry>()
                .Where(a => a.IdUser == idBinusian
                    && a.CheckInDateTime.Date == time.Date)
                .OrderByDescending(a => a.CheckInDateTime)
                .FirstOrDefault();

            var container = GetContainerSasUri(1);

            #region Student
            var isStudent = _context.Entity<MsStudent>()
                .Where(a => a.Id == idBinusian);

            if (isStudent.Any())
            {
                var getStudent = _context.Entity<MsHomeroomStudent>()
                    .Include(a => a.Homeroom.Grade.MsLevel.MsAcademicYear.MsSchool)
                    .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                    .Include(a => a.Student)
                    .Where(a => a.Semester == activeAY.Semester
                        && a.Homeroom.Semester == activeAY.Semester
                        && a.Homeroom.Grade.MsLevel.IdAcademicYear == activeAY.Id
                        && a.Student.Id == idBinusian)
                    .FirstOrDefault();

                var getPhoto = _context.Entity<TrStudentPhoto>()
                    .Where(a => a.IdStudent == idBinusian
                        && a.IdAcademicYear == activeAY.Id)
                    .FirstOrDefault();

                var insertStudent = new UpdateMedicalTappingSystemPatientStatusResponse
                {
                    IdBinusian = idBinusian,
                    ImageUrl = getPhoto != null ? getPhoto.FilePath : GetStudentPhoto(getStudent.Homeroom.Grade.MsLevel.MsAcademicYear.MsSchool.Description, activeAY.Id, getStudent.Student.Id, container),
                    Name = NameUtil.GenerateFullName(getStudent.Student.FirstName, getStudent.Student.LastName),
                    SchoolLevel = new CodeWithIdVm
                    {
                        Id = getStudent.Homeroom.Grade.IdLevel,
                        Description = getStudent.Homeroom.Grade.MsLevel.Description,
                        Code = getStudent.Homeroom.Grade.MsLevel.Code,
                    },
                    Grade = new CodeWithIdVm
                    {
                        Id = getStudent.Homeroom.IdGrade,
                        Description = getStudent.Homeroom.Grade.Description,
                        Code = getStudent.Homeroom.Grade.Code,
                    },
                    Homeroom = new ItemValueVm
                    {
                        Id = getStudent.IdHomeroom,
                        Description = getStudent.Homeroom.Grade.Code + getStudent.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                    },
                    CheckInTime = DateTime.SpecifyKind(new DateTime(getMedicalRecord.CheckInDateTime.Year,
                        getMedicalRecord.CheckInDateTime.Month,
                        getMedicalRecord.CheckInDateTime.Day,
                        getMedicalRecord.CheckInDateTime.Hour,
                        getMedicalRecord.CheckInDateTime.Minute, 0), DateTimeKind.Unspecified),
                    CheckOutTime = getMedicalRecord.CheckOutDateTime.HasValue
                        ? (DateTime?)DateTime.SpecifyKind(new DateTime(getMedicalRecord.CheckOutDateTime.Value.Year,
                        getMedicalRecord.CheckOutDateTime.Value.Month,
                        getMedicalRecord.CheckOutDateTime.Value.Day,
                        getMedicalRecord.CheckOutDateTime.Value.Hour,
                        getMedicalRecord.CheckOutDateTime.Value.Minute, 0), DateTimeKind.Unspecified)
                        : null
                };

                response = insertStudent;
            }
            #endregion

            #region Staff
            var isStaff = _context.Entity<MsStaff>()
                .Where(a => a.IdBinusian == idBinusian);

            if (isStaff.Any())
            {
                var getStaff = _context.Entity<MsStaff>()
                    .Where(a => a.IdBinusian == idBinusian)
                    .FirstOrDefault();

                var insertStaff = new UpdateMedicalTappingSystemPatientStatusResponse
                {
                    IdBinusian = idBinusian,
                    ImageUrl = "",
                    Name = NameUtil.GenerateFullName(getStaff.FirstName, getStaff.LastName),
                    SchoolLevel = new CodeWithIdVm(),
                    Grade = new CodeWithIdVm(),
                    Homeroom = new ItemValueVm(),
                    CheckInTime = DateTime.SpecifyKind(new DateTime(getMedicalRecord.CheckInDateTime.Year,
                        getMedicalRecord.CheckInDateTime.Month,
                        getMedicalRecord.CheckInDateTime.Day,
                        getMedicalRecord.CheckInDateTime.Hour,
                        getMedicalRecord.CheckInDateTime.Minute, 0), DateTimeKind.Unspecified),
                    CheckOutTime = getMedicalRecord.CheckOutDateTime.HasValue
                        ? (DateTime?)DateTime.SpecifyKind(new DateTime(getMedicalRecord.CheckOutDateTime.Value.Year,
                        getMedicalRecord.CheckOutDateTime.Value.Month,
                        getMedicalRecord.CheckOutDateTime.Value.Day,
                        getMedicalRecord.CheckOutDateTime.Value.Hour,
                        getMedicalRecord.CheckOutDateTime.Value.Minute, 0), DateTimeKind.Unspecified)
                        : null
                };

                response = insertStaff;
            }
            #endregion

            #region Other Patient
            var isOtherPatient = _context.Entity<MsMedicalOtherUsers>()
                .Where(a => a.Id == idBinusian
                    && a.IdSchool == request.IdSchool);

            if (isOtherPatient.Any())
            {
                var getOtherPatient = isOtherPatient.FirstOrDefault();

                var insertOtherStaff = new UpdateMedicalTappingSystemPatientStatusResponse
                {
                    IdBinusian = idBinusian,
                    ImageUrl = "",
                    Name = getOtherPatient.MedicalOtherUsersName,
                    SchoolLevel = new CodeWithIdVm(),
                    Grade = new CodeWithIdVm(),
                    Homeroom = new ItemValueVm(),
                    CheckInTime = DateTime.SpecifyKind(new DateTime(getMedicalRecord.CheckInDateTime.Year,
                        getMedicalRecord.CheckInDateTime.Month,
                        getMedicalRecord.CheckInDateTime.Day,
                        getMedicalRecord.CheckInDateTime.Hour,
                        getMedicalRecord.CheckInDateTime.Minute, 0), DateTimeKind.Unspecified),
                    CheckOutTime = getMedicalRecord.CheckOutDateTime.HasValue
                        ? (DateTime?)DateTime.SpecifyKind(new DateTime(getMedicalRecord.CheckOutDateTime.Value.Year,
                        getMedicalRecord.CheckOutDateTime.Value.Month,
                        getMedicalRecord.CheckOutDateTime.Value.Day,
                        getMedicalRecord.CheckOutDateTime.Value.Hour,
                        getMedicalRecord.CheckOutDateTime.Value.Minute, 0), DateTimeKind.Unspecified)
                        : null
                };

                response = insertOtherStaff;
            }
            #endregion

            return response;
        }

        #region Blob Storage
        private static string GetStudentPhoto(string schoolName, string academicYear, string idStudent, string containerLink)
        {
            string url = containerLink.Replace("?", "/" + schoolName + "/" + academicYear + "/" + idStudent + ".jpg" + "?");

            return url;
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_configuration.GetConnectionString("Student:AcountStorage"));
                return storageAccount;
            }
            catch
            {
                var storageAccount = CloudStorageAccount.Parse(_configuration["ConnectionStrings:Student:AccountStorage"]);
                return storageAccount;
            }
        }

        private string GetContainerSasUri(int expiryHour, string storedPolicyName = null)
        {
            string sasContainerToken;

            CloudBlobContainer container;

            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference("studentphoto");

            if (storedPolicyName == null)
            {
                SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(expiryHour),
                    Permissions = SharedAccessBlobPermissions.Read,
                };

                sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);
            }
            else
            {
                sasContainerToken = container.GetSharedAccessSignature(null, storedPolicyName);
            }

            return container.Uri + sasContainerToken;
        }
        #endregion
    }
}
