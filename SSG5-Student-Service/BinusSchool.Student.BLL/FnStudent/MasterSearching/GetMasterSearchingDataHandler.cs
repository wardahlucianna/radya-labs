using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Domain.Extensions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollmentDetail;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;

namespace BinusSchool.Student.FnStudent.MasterSearching
{
    public class GetMasterSearchingDataHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IStudentEnrollmentDetail _studentEnrollmentService;
        private readonly IConfiguration _configuration;
        public GetMasterSearchingDataHandler(IStudentDbContext schoolDbContext,
                                            IStudentEnrollmentDetail StudentEnrollmentService,
                                            IConfiguration configuration)
        {
            _dbContext = schoolDbContext;
            _studentEnrollmentService = StudentEnrollmentService;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<GetMasterSearchingDataRequest>();

            #region cara baru

            //var paramForStudentEnrollment = new GetStudentEnrollmentforStudentApprovalSummaryRequest
            //{
            //    AcademicYearId = param.AcademicYear,
            //    SchoolId = param.SchoolID,
            //    GradeId = param.YearLevelId,
            //    PathwayID = param.HomeroomID
            //};

            //var studentEnrollment = await _studentEnrollmentService.GetStudentEnrollmentForStudentApprovalSummary(paramForStudentEnrollment);

            var studentEnrollment = await _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(x => x.Homeroom).
                                        ThenInclude(a => a.Grade).
                                        ThenInclude(b => b.MsLevel).
                                        ThenInclude(c => c.MsAcademicYear)
                                        .Include(x => x.Student)
                                        .Where(x => x.Homeroom.Grade.MsLevel.IdAcademicYear == (string.IsNullOrEmpty(param.AcademicYear) ? x.Homeroom.Grade.MsLevel.IdAcademicYear : param.AcademicYear)
                                        && x.Homeroom.Grade.MsLevel.MsAcademicYear.IdSchool == (string.IsNullOrEmpty(param.SchoolID) ? x.Homeroom.Grade.MsLevel.MsAcademicYear.IdSchool : param.SchoolID)
                                        && x.Homeroom.Grade.IdLevel == (string.IsNullOrEmpty(param.SchoolLevelId) ? x.Homeroom.Grade.IdLevel : param.SchoolLevelId)
                                        && x.Homeroom.IdGrade == (string.IsNullOrEmpty(param.YearLevelId) ? x.Homeroom.IdGrade : param.YearLevelId)
                                        && x.IdHomeroom == (string.IsNullOrEmpty(param.HomeroomID) ? x.IdHomeroom : param.HomeroomID)
                                        )
                                        .Select(x => new GetStudentEnrollmentforStudentApprovalSummaryResult
                                        {
                                            AcademicYearId = param.AcademicYear,
                                            GradeId = x.Homeroom.IdGrade,
                                            GradeName = x.Homeroom.Grade.Code,
                                            StudentId = x.IdStudent
                                        }).ToListAsync();

            var container = GetContainerSasUri(1);

            if (studentEnrollment != null)
            {
                if (param.GetAll != null && param.GetAll == false)
                {
                    #region Create Dynamic Where

                    var predicate = PredicateBuilder.False<GetMasterSearchingDataResult>();

                    #region student
                    if (!string.IsNullOrEmpty(param.BinusianID))
                    {
                        predicate = predicate.Or(s => s.BinusianID.Contains(param.BinusianID));
                    }

                    if (!string.IsNullOrEmpty(param.StudentName))
                    {
                        predicate = predicate.Or(s => s.StudentName.Contains(param.StudentName));
                    }

                    if (!string.IsNullOrEmpty(param.ReligionName))
                    {
                        predicate = predicate.Or(s => s.ReligionName.Contains(param.ReligionName));
                    }

                    if (!string.IsNullOrEmpty(param.BinusEmailAddress))
                    {
                        predicate = predicate.Or(s => s.BinusEmailAddress.Contains(param.BinusEmailAddress));
                    }
                    #endregion

                    #region father
                    if (!string.IsNullOrEmpty(param.FatherName))
                    {
                        predicate = predicate.Or(s => s.FatherName.Contains(param.FatherName));
                    }

                    if (!string.IsNullOrEmpty(param.FatherMobilePhoneNumber1))
                    {
                        predicate = predicate.Or(s => s.FatherMobilePhoneNumber1.Contains(param.FatherMobilePhoneNumber1));
                    }

                    if (!string.IsNullOrEmpty(param.FatherResidenceAddress))
                    {
                        predicate = predicate.Or(s => s.FatherResidenceAddress.Contains(param.FatherResidenceAddress));
                    }

                    if (!string.IsNullOrEmpty(param.FatherEmailAddress))
                    {
                        predicate = predicate.Or(s => s.FatherEmailAddress.Contains(param.FatherEmailAddress));
                    }

                    if (!string.IsNullOrEmpty(param.FatherCompanyName))
                    {
                        predicate = predicate.Or(s => s.FatherCompanyName.Contains(param.FatherCompanyName));
                    }

                    if (!string.IsNullOrEmpty(param.FatherOccupationPosition))
                    {
                        predicate = predicate.Or(s => s.FatherOccupationPosition.Contains(param.FatherOccupationPosition));
                    }

                    if (!string.IsNullOrEmpty(param.FatherOfficeEmail))
                    {
                        predicate = predicate.Or(s => s.FatherOfficeEmail.Contains(param.FatherOfficeEmail));
                    }
                    #endregion

                    #region Mother
                    if (!string.IsNullOrEmpty(param.MotherName))
                    {
                        predicate = predicate.Or(s => s.MotherName.Contains(param.MotherName));
                    }

                    if (!string.IsNullOrEmpty(param.MotherMobilePhoneNumber1))
                    {
                        predicate = predicate.Or(s => s.MotherMobilePhoneNumber1.Contains(param.MotherMobilePhoneNumber1));
                    }

                    if (!string.IsNullOrEmpty(param.MotherResidenceAddress))
                    {
                        predicate = predicate.Or(s => s.MotherResidenceAddress.Contains(param.MotherResidenceAddress));
                    }

                    if (!string.IsNullOrEmpty(param.MotherEmailAddress))
                    {
                        predicate = predicate.Or(s => s.MotherEmailAddress.Contains(param.MotherEmailAddress));
                    }

                    if (!string.IsNullOrEmpty(param.MotherCompanyName))
                    {
                        predicate = predicate.Or(s => s.MotherCompanyName.Contains(param.MotherCompanyName));
                    }

                    if (!string.IsNullOrEmpty(param.MotherOccupationPosition))
                    {
                        predicate = predicate.Or(s => s.MotherOccupationPosition.Contains(param.MotherOccupationPosition));
                    }

                    if (!string.IsNullOrEmpty(param.MotherOfficeEmail))
                    {
                        predicate = predicate.Or(s => s.MotherOfficeEmail.Contains(param.MotherOfficeEmail));
                    }
                    #endregion

                    #endregion

                    var studentEnrollmentResult = studentEnrollment;

                    if (studentEnrollmentResult != null && studentEnrollmentResult.Count > 0)
                    {
                        var StudentList = studentEnrollmentResult.Select(x => x.StudentId).ToList();
                        if (string.IsNullOrEmpty(param.AcademicYear))
                        {

                            var query = _dbContext.Entity<MsStudent>()
                                    .Include(x => x.Religion)
                                    .Include(x => x.Nationality)
                                    .Include(x => x.StudentParents)
                                    .Include(x => x.StudentPrevSchoolInfo)
                                    .Where(x => StudentList.Contains(x.Id))
                                    .Select(
                                        x => new GetMasterSearchingDataResult
                                        {
                                            #region Student Data
                                            //Photo = GetStudentPhoto(param.SchoolName, studentEnrollmentResult.Where(y => y.StudentId == x.Id).Select(x => x.AcademicYearId).FirstOrDefault(), x.Id, container),
                                            Photo = "",
                                            SchoolName = param.SchoolName,
                                            StudentStatusID = x.IdStudentStatus.ToString(),
                                            SchoolID = x.IdSchool,
                                            BinusianID = x.Id,
                                            StudentName = (string.IsNullOrEmpty(x.FirstName.Trim()) ? "" : x.FirstName) + " "
                                                        + (string.IsNullOrEmpty(x.LastName.Trim()) ? "" : x.LastName),
                                            Gender = x.Gender,
                                            ReligionID = x.IdReligion,
                                            ReligionName = x.Religion.ReligionName,
                                            DOB = Convert.ToDateTime(x.DOB).ToString("yyyy-MM-dd"),
                                            BinusEmailAddress = x.BinusianEmailAddress,
                                            Nationality = x.Nationality.NationalityName,
                                            //Previous school need to create checker 
                                            PreviousSchool = (string.IsNullOrEmpty(x.StudentPrevSchoolInfo.IdPreviousSchoolNew) ? x.StudentPrevSchoolInfo.PreviousSchoolOld.SchoolName : x.StudentPrevSchoolInfo.PreviousSchoolNew.SchoolName),
                                            #endregion

                                            #region Father Data
                                            FatherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
                                            FatherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
                                            FatherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
                                            FatherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
                                            FatherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.CompanyName.Trim()) ? "" : z.Parent.CompanyName)).FirstOrDefault(),
                                            FatherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.OccupationPosition).FirstOrDefault(),
                                            FatherOfficeEmail = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.WorkEmailAddress).FirstOrDefault(),
                                            #endregion

                                            #region Mother Data
                                            MotherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
                                            MotherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
                                            MotherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
                                            MotherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
                                            MotherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.CompanyName).FirstOrDefault(),
                                            MotherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.OccupationPosition).FirstOrDefault()
                                            #endregion

                                        }
                                            )
                                            .Where(predicate)
                                            .Where(x => (param.StudentStatusID == "0" ? x.StudentStatusID == x.StudentStatusID : x.StudentStatusID == param.StudentStatusID));

                            var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                            var count = await query.CountAsync(CancellationToken);
                            //return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
                            return Request.CreateApiResult2(items as object);
                        }
                        else
                        {
                            var query = _dbContext.Entity<MsStudent>()
                                    .Include(x => x.Religion)
                                    .Include(x => x.Nationality)
                                    .Include(x => x.StudentParents)
                                    .Include(x => x.StudentPrevSchoolInfo)
                                    .Where(x => StudentList.Contains(x.Id))
                                    .Select(
                                        x => new GetMasterSearchingDataResult
                                        {
                                            #region Student Data
                                            Photo = GetStudentPhoto(param.SchoolName, param.AcademicYear, x.Id, container),
                                            SchoolName = param.SchoolName,
                                            StudentStatusID = x.IdStudentStatus.ToString(),
                                            SchoolID = x.IdSchool,
                                            BinusianID = x.Id,
                                            StudentName = (string.IsNullOrEmpty(x.FirstName.Trim()) ? "" : x.FirstName) + " "
                                                        + (string.IsNullOrEmpty(x.LastName.Trim()) ? "" : x.LastName),
                                            Gender = x.Gender,
                                            ReligionID = x.IdReligion,
                                            ReligionName = x.Religion.ReligionName,
                                            DOB = Convert.ToDateTime(x.DOB).ToString("yyyy-MM-dd"),
                                            BinusEmailAddress = x.BinusianEmailAddress,
                                            Nationality = x.Nationality.NationalityName,
                                            //Previous school need to create checker 
                                            PreviousSchool = (string.IsNullOrEmpty(x.StudentPrevSchoolInfo.IdPreviousSchoolNew) ? x.StudentPrevSchoolInfo.PreviousSchoolOld.SchoolName : x.StudentPrevSchoolInfo.PreviousSchoolNew.SchoolName),
                                            #endregion

                                            #region Father Data
                                            FatherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
                                            FatherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
                                            FatherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
                                            FatherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
                                            FatherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.CompanyName.Trim()) ? "" : z.Parent.CompanyName)).FirstOrDefault(),
                                            FatherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.OccupationPosition).FirstOrDefault(),
                                            FatherOfficeEmail = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.WorkEmailAddress).FirstOrDefault(),
                                            #endregion

                                            #region Mother Data
                                            MotherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
                                            MotherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
                                            MotherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
                                            MotherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
                                            MotherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.CompanyName).FirstOrDefault(),
                                            MotherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.OccupationPosition).FirstOrDefault()
                                            #endregion

                                        }
                                            )
                                            .Where(predicate)
                                            .Where(x => (param.StudentStatusID == "0" ? x.StudentStatusID == x.StudentStatusID : x.StudentStatusID == param.StudentStatusID));

                            var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);
                            var count = await query.CountAsync(CancellationToken);
                            //return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
                            return Request.CreateApiResult2(items as object);
                        }
                    }
                    else
                    {
                        var item = new List<GetMasterSearchingDataResult>();

                        var count = item.Count;

                        //return Request.CreateApiResult2(item as object, param.CreatePaginationProperty(count));
                        return Request.CreateApiResult2(item as object);
                    }

                }
                //Get All
                else
                {
                    param.GetAll = false;

                    var studentEnrollmentResult = studentEnrollment;

                    if (studentEnrollmentResult != null && studentEnrollmentResult.Count > 0)
                    {
                        //var StudentList = studentEnrollmentResult.Select(x => new MsStudent { Id = x.StudentId }).ToList();

                        var StudentList = studentEnrollmentResult.Select(x => x.StudentId).ToList();

                        var query = _dbContext.Entity<MsStudent>()
                                .Include(x => x.Religion)
                                .Include(x => x.Nationality)
                                .Include(x => x.StudentParents)
                                .Include(x => x.StudentPrevSchoolInfo)
                                .Where(x => (param.StudentStatusID == "0" ? x.IdStudentStatus.ToString() == x.IdStudentStatus.ToString() : x.IdStudentStatus.ToString() == param.StudentStatusID.ToString()))
                                .Where(x => StudentList.Contains(x.Id));
                        //.Intersect(StudentList);

                        var items = await query.Select(
                            x => new GetMasterSearchingDataResult
                            {
                                        #region Student Data
                                        Photo = GetStudentPhoto(param.SchoolName, param.AcademicYear, x.Id, container),
                                StudentStatusID = x.IdStudentStatus.ToString(),
                                SchoolName = param.SchoolName,
                                BinusianID = x.Id,
                                StudentName = (string.IsNullOrEmpty(x.FirstName.Trim()) ? "" : x.FirstName) + " "
                                            + (string.IsNullOrEmpty(x.LastName.Trim()) ? "" : x.LastName),
                                Gender = x.Gender,
                                ReligionID = x.IdReligion,
                                ReligionName = x.Religion.ReligionName,
                                DOB = Convert.ToDateTime(x.DOB).ToString("yyyy-MM-dd"),
                                BinusEmailAddress = x.BinusianEmailAddress,
                                Nationality = x.Nationality.NationalityName,
                                        //PreviousSchool = x.StudentPrevSchoolInfo.MasterSchoolName,
                                        PreviousSchool = (string.IsNullOrEmpty(x.StudentPrevSchoolInfo.IdPreviousSchoolNew) ? x.StudentPrevSchoolInfo.PreviousSchoolOld.SchoolName : x.StudentPrevSchoolInfo.PreviousSchoolNew.SchoolName),
                                        #endregion

                                        #region Father Data
                                        FatherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
                                FatherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
                                FatherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
                                FatherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
                                FatherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.CompanyName.Trim()) ? "" : z.Parent.CompanyName)).FirstOrDefault(),
                                FatherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.OccupationPosition).FirstOrDefault(),
                                FatherOfficeEmail = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.WorkEmailAddress).FirstOrDefault(),
                                        #endregion

                                        #region Mother Data
                                        MotherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
                                MotherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
                                MotherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
                                MotherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
                                MotherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.CompanyName).FirstOrDefault(),
                                MotherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.OccupationPosition).FirstOrDefault()
                                        #endregion

                                    }
                                ).OrderByDynamic(param).ToListAsync(CancellationToken);

                        var count = await query.CountAsync(CancellationToken);
                        //return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
                        return Request.CreateApiResult2(items as object);
                    }
                    else
                    {
                        var item = new List<GetMasterSearchingDataResult>();

                        var count = item.Count;

                        //return Request.CreateApiResult2(item as object, param.CreatePaginationProperty(count));
                        return Request.CreateApiResult2(item as object);
                    }
                }

            }
            else
            {
                return Request.CreateApiResult2();
            }
            #endregion

        }

        public static string GetStudentPhoto(string schoolname, string academicYear, string studentId, string containerLink)
        {
            string url = containerLink.Replace("?", "/" + schoolname + "/" + academicYear + "/" + studentId + ".jpg" + "?");

            return url;
        }

        private CloudStorageAccount GetCloudStorageAccount()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_configuration.GetConnectionString("Student:AccountStorage"));
                return storageAccount;
            }
            catch
            {
                var storageAccount = CloudStorageAccount.Parse(Configuration["ConnectionStrings:Student:AccountStorage"]);
                return storageAccount;
            }
        }

        public string GetContainerSasUri(int expiryHour, string storedPolicyName = null)
        {
            string sasContainerToken;

            CloudBlobContainer container;


            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference("studentphoto");

            // If no stored policy is specified, create a new access policy and define its constraints.
            if (storedPolicyName == null)
            {
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
                // to construct a shared access policy that is saved to the container's shared access policies.
                SharedAccessBlobPolicy adHocPolicy = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(expiryHour),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                // Generate the shared access signature on the container, setting the constraints directly on the signature.
                sasContainerToken = container.GetSharedAccessSignature(adHocPolicy, null);
            }
            else
            {
                // Generate the shared access signature on the container. In this case, all of the constraints for the
                // shared access signature are specified on the stored access policy, which is provided by name.
                // It is also possible to specify some constraints on an ad hoc SAS and others on the stored access policy.
                sasContainerToken = container.GetSharedAccessSignature(null, storedPolicyName);
            }

            // Return the URI string for the container, including the SAS token.
            return container.Uri + sasContainerToken;

            //Return blob SAS Token
            //return sasContainerToken;
        }

        private string GetBlobSasUri(string blobName, int expiryHour, string policyName = null)
        {
            string sasBlobToken;

            CloudBlobContainer container;


            //if (AppType == 1)
            //{
            CloudStorageAccount storageAccount = GetCloudStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            container = blobClient.GetContainerReference("studentphoto");

            // Get a reference to a blob within the container.
            // Note that the blob may not exist yet, but a SAS can still be created for it.
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            if (policyName == null)
            {
                // Create a new access policy and define its constraints.
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad hoc SAS, and
                // to construct a shared access policy that is saved to the container's shared access policies.
                SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(expiryHour),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                // Generate the shared access signature on the blob, setting the constraints directly on the signature.
                sasBlobToken = blob.GetSharedAccessSignature(adHocSAS);
            }
            else
            {
                // Generate the shared access signature on the blob. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                sasBlobToken = blob.GetSharedAccessSignature(null, policyName);
            }

            // Return the URI string for the container, including the SAS token.
            //return blob.Uri + sasBlobToken;

            //Return blob SAS Token
            return sasBlobToken;
        }

    }
}
