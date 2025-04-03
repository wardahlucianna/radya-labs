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
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Student.FnStudent.MedicalSystem.Helper;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry
{
    public class GetMedicalRecordEntryDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IMachineDateTime _time;
        private readonly IPeriod _period;
        private readonly IConfiguration _configuration;

        public GetMedicalRecordEntryDetailHandler(IStudentDbContext context, IMachineDateTime time, IPeriod period, IConfiguration configuration)
        {
            _context = context;
            _time = time;
            _period = period;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetMedicalRecordEntryDetailRequest>
                (nameof(GetMedicalRecordEntryDetailRequest.IdSchool),
                 nameof(GetMedicalRecordEntryDetailRequest.Id),
                 nameof(GetMedicalRecordEntryDetailRequest.Mode));

            var response = await GetMedicalRecordEntryDetail(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<GetMedicalRecordEntryDetailResponse> GetMedicalRecordEntryDetail(GetMedicalRecordEntryDetailRequest request)
        {
            var response = new GetMedicalRecordEntryDetailResponse();
            DateTime time = _time.ServerTime;

            var idBinusian = MedicalDecryptionValidation.ValidateDecryptionData(request.Id);

            var getCurrentAY = await _period.GetCurrenctAcademicYear(new Data.Model.School.FnPeriod.Period.CurrentAcademicYearRequest
            {
                IdSchool = request.IdSchool,
            });

            var activeAY = getCurrentAY.Payload;

            #region Visit Summary
            int runningMonth = _context.Entity<TrMedicalRecordEntry>()
                .Where(a => a.IdUser == idBinusian
                    && a.CheckInDateTime.Month == time.Month)
                .Count();

            int runningAccumulative = _context.Entity<TrMedicalRecordEntry>()
                .Where(a => a.IdUser == idBinusian)
                .Count();
            #endregion

            if (request.Mode.ToLower() == "student")
            {
                var container = GetContainerSasUri(1);

                var getStudent = _context.Entity<MsHomeroomStudent>()
                    .Include(a => a.Student)
                    .Include(a => a.Homeroom.Grade.MsLevel.MsAcademicYear.MsSchool)
                    .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                    .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == activeAY.Id
                        && a.Semester == activeAY.Semester
                        && a.Homeroom.Semester == activeAY.Semester
                        && a.Student.Id == idBinusian)
                    .FirstOrDefault();

                var getPhoto = _context.Entity<TrStudentPhoto>()
                    .Where(a => a.IdStudent == idBinusian
                        && a.IdAcademicYear == activeAY.Id)
                    .FirstOrDefault();

                var insertStudent = new GetMedicalRecordEntryDetailResponse
                {
                    IdBinusian = new ItemValueVm
                    {
                        Id = AESCBCEncryptionUtil.EncryptBase64Url($"{getStudent.Student.Id}#{time.ToString("ddMMyyyy")}"),
                        Description = getStudent.Student.Id
                    },
                    Name = NameUtil.GenerateFullName(getStudent.Student.FirstName, getStudent.Student.LastName),
                    BirthDate = getStudent.Student.DOB.Value.Date,
                    BirthPlace = getStudent.Student.POB,
                    Age = GetAgeString(getStudent.Student.DOB.Value.Date),
                    ClinicVisitation = new GetMedicalRecordEntryDetailResponse_ClinicVisit
                    {
                        RunningMonth = runningMonth,
                        RunningAccumulative = runningAccumulative,
                    },
                    ImageUrl = getPhoto != null ? getPhoto.FilePath : GetStudentPhoto(getStudent.Homeroom.Grade.MsLevel.MsAcademicYear.MsSchool.Description, activeAY.Id, getStudent.Student.Id, container),
                    Gender = getStudent.Student.Gender.GetDescription(),
                    Grade = new CodeWithIdVm
                    {
                        Id = getStudent.Homeroom.Grade.Id,
                        Description = getStudent.Homeroom.Grade.Description,
                        Code = getStudent.Homeroom.Grade.Code
                    },
                    Homeroom = new ItemValueVm
                    {
                        Id = getStudent.Homeroom.Id,
                        Description = getStudent.Homeroom.Grade.Code + getStudent.Homeroom.MsGradePathwayClassroom.Classroom.Code
                    }
                };

                response = insertStudent;
            }
            else if (request.Mode.ToLower() == "staff")
            {
                var getStaff = _context.Entity<MsStaff>()
                    .Where(a => a.IdBinusian == idBinusian
                        && a.IdSchool == request.IdSchool)
                    .FirstOrDefault();

                var insertStaff = new GetMedicalRecordEntryDetailResponse
                {
                    IdBinusian = new ItemValueVm
                    {
                        Id = AESCBCEncryptionUtil.EncryptBase64Url($"{getStaff.IdBinusian}#{time.ToString("ddMMyyyy")}"),
                        Description = getStaff.IdBinusian
                    },
                    Name = NameUtil.GenerateFullName(getStaff.FirstName, getStaff.LastName),
                    BirthDate = null,
                    BirthPlace = null,
                    Age = null,
                    ClinicVisitation = new GetMedicalRecordEntryDetailResponse_ClinicVisit
                    {
                        RunningMonth = runningMonth,
                        RunningAccumulative = runningAccumulative,
                    },
                    ImageUrl = null,
                    Gender = null,
                    Grade = new CodeWithIdVm(),
                    Homeroom = new ItemValueVm()
                };

                response = insertStaff;
            }
            else
            {
                var getOtherPatient = _context.Entity<MsMedicalOtherUsers>()
                    .Where(a => a.IdSchool == request.IdSchool
                        && a.Id == idBinusian)
                    .FirstOrDefault();

                var insertOtherPatient = new GetMedicalRecordEntryDetailResponse
                {
                    IdBinusian = new ItemValueVm
                    {
                        Id = AESCBCEncryptionUtil.EncryptBase64Url($"{getOtherPatient.Id}#{time.ToString("ddMMyyyy")}"),
                        Description = getOtherPatient.Id
                    },
                    Name = NameUtil.GenerateFullName(getOtherPatient.MedicalOtherUsersName),
                    BirthDate = getOtherPatient.BirthDate.Date,
                    BirthPlace = null,
                    Age = GetAgeString(getOtherPatient.BirthDate.Date),
                    ClinicVisitation = new GetMedicalRecordEntryDetailResponse_ClinicVisit
                    {
                        RunningMonth = runningMonth,
                        RunningAccumulative = runningAccumulative,
                    },
                    ImageUrl = null,
                    Gender = null,
                    Grade = new CodeWithIdVm(),
                    Homeroom = new ItemValueVm()
                };

                response = insertOtherPatient;
            }

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

        private string GetAgeString(DateTime dob)
        {
            DateTime time = _time.ServerTime;

            int years = time.Year - dob.Year;
            int months = time.Month - dob.Month;
            int days = time.Day - dob.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(time.Year, time.Month == 1 ? 12 : time.Month - 1);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            return $"({years} years {months} months {days} days)";
        }
    }
}
