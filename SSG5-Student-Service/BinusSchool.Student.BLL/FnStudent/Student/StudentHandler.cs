using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Model.Information;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentHomeroomDetail;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.Student.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;

namespace BinusSchool.Student.FnStudent.Student
{
    public class StudentHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbStudentContext;
        private readonly int _newEntryApproval;
        private IDbContextTransaction _transaction;
        private readonly IConfiguration _configuration;

        public StudentHandler(IStudentDbContext dbStudentContext
            , IConfiguration configuration
        )
        {
            _dbStudentContext = dbStudentContext;
            _newEntryApproval = Convert.ToInt32(configuration["NewEntryApproval"]);
            _configuration = configuration;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
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

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var container = GetContainerSasUri(1);

            var studentData = await _dbStudentContext.Entity<MsStudent>()
                .Where(x => x.Id == id)
                .Select(x => new { x.IdAddressCountry, x.IdAddressCity, x.IdAddressStateProvince, x.IdBirthCountry, x.IdBirthCity, x.IdBirthStateProvince, x.IdSchool, x.EmergencyContactRole })
                .FirstOrDefaultAsync(CancellationToken);

            if (studentData is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["student"], "Id", id));

            var studentHomeroomDetail = await _dbStudentContext.Entity<MsHomeroomStudent>()
                .Where(x => x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.MsAcademicYear.IdSchool == studentData.IdSchool
                //.Where(x => x.IdStudent == id && x.Homeroom.Semester == GetAcademicYear.Semester)
                //.Where(x => x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == GetAcademicYear.AcademicYear.Id
                && x.IdStudent == id)
                .OrderByDescending(a => a.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.MsAcademicYear.Code)
                .ThenByDescending(a => a.Semester)
                .Select(x => new GetStudentHomeroomDetailResult
                {
                    StudentId = id,
                    SchoolId = x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.MsAcademicYear.IdSchool,
                    AcadYear = new CodeWithIdVm()
                    {
                        Id = x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear,
                        Code = x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.MsAcademicYear.Code,
                        Description = x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.MsAcademicYear.Description,
                    },
                    Semester = x.Homeroom.Semester,
                    Level = new CodeWithIdVm()
                    {
                        Id = x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Id,
                        Code = x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Code,
                        Description = x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Description,
                    },
                    Grade = new CodeWithIdVm()
                    {
                        Id = x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Id,
                        Code = x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code,
                        Description = x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Description,
                    },
                    Class = new CodeWithIdVm()
                    {
                        Description = x.Homeroom.Grade.Code + "" + x.Homeroom.MsGradePathwayClassroom.Classroom.Code
                    }
                })
                .FirstOrDefaultAsync();

            if (studentHomeroomDetail is null)
                throw new BadRequestException(string.Format(Localizer["Student has not enrolled yet"], Localizer["student"], "Id", id));

            var siblingGroupId = await _dbStudentContext.Entity<MsSiblingGroup>()
                                .Where(x => x.IdStudent == id)
                                .Select(x => x.Id)
                                .FirstOrDefaultAsync(CancellationToken);

            var school = _dbStudentContext.Entity<MsSchool>().Where(x => x.Id == studentData.IdSchool).FirstOrDefault();

            var acadYear = studentHomeroomDetail == null ? null : studentHomeroomDetail.AcadYear;
            var gradeResult = studentHomeroomDetail == null ? null : studentHomeroomDetail.Grade;
            var schoolLevel = studentHomeroomDetail == null ? null : studentHomeroomDetail.Level;
            var ClassResult = studentHomeroomDetail == null ? null : studentHomeroomDetail.Class;

            var statusApproval = _dbStudentContext.Entity<TrStudentInfoUpdate>()
                                .Where(x => x.Constraint3Value == id && x.IdApprovalStatus == _newEntryApproval && x.Constraint1Value == "Add" && x.TableName == "MsStudentPrevSchoolInfo")
                                .Select(x => new { id = x.Constraint3Value, description = x.Constraint1Value })
                                .Distinct()
                                .FirstOrDefault();

            var studentPrevSchool = _dbStudentContext.Entity<MsStudentPrevSchoolInfo>()
                                .Where(x => x.IdStudent == id)
                                .Select(x => new//GetStudentPrevSchoolInfoData
                                {
                                    Grade = x.Grade,
                                    YearAttended = x.YearAttended,
                                    YearWithdrawn = x.YearWithdrawn,
                                    IsHomeSchooling = x.IsHomeSchooling,
                                    IdPreviousSchoolNew = x.IdPreviousSchoolNew,
                                    IdPreviousSchoolOld = x.IdPreviousSchoolOld
                                })
                                .FirstOrDefault();

            var emergencyContactRole = _dbStudentContext.Entity<MsStudentParent>()
                              .Include(x => x.Parent)
                              .Where(x => x.IdStudent == id && x.Parent.IdParentRole == studentData.EmergencyContactRole)
                              .Select(x => new { x.Parent.ParentRole.ParentRoleNameEng, x.Parent.Id, x.Parent.IdParentRole, x.Parent.FirstName, x.Parent.LastName, x.Parent.MobilePhoneNumber1, x.Parent.PersonalEmailAddress })
                              .FirstOrDefault();

            var emergencyContactRoleDetail = new EmergencyContactInfoDetailVm
            {
                IdParent = emergencyContactRole == null ? null : emergencyContactRole.Id,
                EmergencyContact = new ItemValueVm
                {
                    Id = emergencyContactRole == null ? null : emergencyContactRole.IdParentRole,
                    Description = emergencyContactRole == null ? null : emergencyContactRole.ParentRoleNameEng,
                },
                EmergencyContactName = emergencyContactRole == null ? null : emergencyContactRole.FirstName + " " + emergencyContactRole.LastName,
                EmergencyContactNumber = emergencyContactRole == null ? null : emergencyContactRole.MobilePhoneNumber1,
                EmergencyEmail = emergencyContactRole == null ? null : emergencyContactRole.PersonalEmailAddress,
            };
            var bankVAAccount = _dbStudentContext.Entity<MsBankAccountInformation>()
                                .Where(x => x.IdStudent == id /*&& x.Status == 1*/)
                                .Select(x => new { x.IdBank, x.AccountNumberCurrentValue, x.AccountNameCurrentValue, x.BankAccountNameCurrentValue })
                                .FirstOrDefault();

            var bankName = _dbStudentContext.Entity<MsBank>()
                                .Where(x => x.Id == (bankVAAccount == null ? null : bankVAAccount.IdBank))
                                .Select(x => new { id = x.Id, description = x.BankName })
                                .FirstOrDefault();

            var bankNameInfo = new ItemValueVm
            {
                Id = bankName == null ? null : bankName.id,
                Description = bankName == null ? null : bankName.description,
            };

            var addressCountry = _dbStudentContext.Entity<LtCountry>()
                                .Where(x => x.Id == (studentData == null ? null : studentData.IdAddressCountry))
                                .Select(x => new { id = x.Id, description = x.CountryName })
                                .FirstOrDefault();

            var addressProvince = _dbStudentContext.Entity<LtProvince>()
                                .Where(x => x.Id == (studentData == null ? null : studentData.IdAddressStateProvince)
                                && x.IdCountry == (addressCountry == null ? null : addressCountry.id))
                                .Select(x => new { id = x.Id, description = x.ProvinceName })
                                .FirstOrDefault();

            var addressCity = _dbStudentContext.Entity<LtCity>()
                                .Where(x => x.Id == (studentData == null ? null : studentData.IdAddressCity)
                                && x.IdCountry == (addressCountry == null ? null : addressCountry.id)
                                && x.IdProvince == (addressProvince == null ? null : addressProvince.id))
                                .Select(x => new { id = x.Id, description = x.CityName })
                                .FirstOrDefault();

            var birthCountry = _dbStudentContext.Entity<LtCountry>()
                                .Where(x => x.Id == (studentData == null ? null : studentData.IdBirthCountry))
                                .Select(x => new { id = x.Id, description = x.CountryName })
                                .FirstOrDefault();

            var birthProvince = _dbStudentContext.Entity<LtProvince>()
                                .Where(x => x.Id == (studentData == null ? null : studentData.IdBirthStateProvince)
                                && x.IdCountry == (birthCountry == null ? null : birthCountry.id))
                                .Select(x => new { id = x.Id, description = x.ProvinceName })
                                .FirstOrDefault();

            var birthCity = _dbStudentContext.Entity<LtCity>()
                                .Where(x => x.Id == (studentData == null ? null : studentData.IdBirthCity)
                                && x.IdCountry == (birthCountry == null ? null : birthCountry.id)
                                && x.IdProvince == (birthProvince == null ? null : birthProvince.id))
                                .Select(x => new { id = x.Id, description = x.CityName })
                                .FirstOrDefault();

            if (statusApproval != null)
            {
                studentPrevSchool = null;
                studentPrevSchool =
                            new
                            {
                                Grade = "Process Add",
                                YearAttended = "Process Add",
                                YearWithdrawn = "Process Add",
                                IsHomeSchooling = (short)0,
                                IdPreviousSchoolNew = "Process Add",
                                IdPreviousSchoolOld = "Process Add"
                            };
            }

            var prevSchoolNewName = _dbStudentContext.Entity<MsPreviousSchoolNew>()
                                .Where(x => x.Id == (studentPrevSchool == null ? null : studentPrevSchool.IdPreviousSchoolNew))
                                .Select(x => new { id = x.Id, description = x.SchoolName })
                                .FirstOrDefault();

            var prevSchoolNewNameInfo = new ItemValueVm
            {
                Id = prevSchoolNewName == null ? null : prevSchoolNewName.id,
                Description = prevSchoolNewName == null ? null : prevSchoolNewName.description,
            };

            var prevSchoolNew = new PreviousSchoolInfoVm
            {
                Grade = studentPrevSchool == null ? null : studentPrevSchool.Grade,
                YearAttended = studentPrevSchool == null ? null : studentPrevSchool.YearAttended,
                YearWithdrawn = studentPrevSchool == null ? null : studentPrevSchool.YearWithdrawn,
                IsHomeSchooling = studentPrevSchool == null ? (short)0 : studentPrevSchool.IsHomeSchooling,
                IdPreviousSchoolNew = prevSchoolNewNameInfo
            };

            var schoolInfo = new ItemValueVm
            {
                Id = school == null ? null : school.Id,
                Description = school == null ? null : school.Name,
            };

            var addressCountryInfo = new ItemValueVm
            {
                Id = addressCountry == null ? null : addressCountry.id,
                Description = addressCountry == null ? null : addressCountry.description,
            };

            var birthCountryInfo = new ItemValueVm
            {
                Id = birthCountry == null ? null : birthCountry.id,
                Description = birthCountry == null ? null : birthCountry.description,
            };

            var addressCityInfo = new ItemValueVm
            {
                Id = addressCity == null ? null : addressCity.id,
                Description = addressCity == null ? null : addressCity.description
            };

            var addressProvinceInfo = new ItemValueVm
            {
                Id = addressProvince == null ? null : addressProvince.id,
                Description = addressProvince == null ? null : addressProvince.description,
            };

            var birthCityInfo = new ItemValueVm
            {
                Id = birthCity == null ? null : birthCity.id,
                Description = birthCity == null ? null : birthCity.description
            };

            var birthProvinceInfo = new ItemValueVm
            {
                Id = birthProvince == null ? null : birthProvince.id,
                Description = birthProvince == null ? null : birthProvince.description
            };

            IReadOnlyList<string> siblingGroups = default;

            siblingGroups = _dbStudentContext.Entity<MsSiblingGroup>()
                            .Where(x => x.Id == siblingGroupId)
                            .Select(x => x.IdStudent)
                            .ToList();

            var query = await _dbStudentContext.Entity<MsStudent>()
                    .Include(x => x.StudentParents)
                        .ThenInclude(y => y.Parent).ThenInclude(z => z.ParentRole)
                    .Include(x => x.StudentParents)
                        .ThenInclude(y => y.Parent).ThenInclude(z => z.OccupationType)
                    .Include(x => x.StudentParents)
                        .ThenInclude(y => y.Parent).ThenInclude(z => z.ParentSalaryGroup)
                    .Include(x => x.Country)
                    .Include(x => x.Religion)
                    .Include(x => x.Nationality)
                    .Include(x => x.ReligionSubject)
                    .Include(x => x.ChildStatus)
                    .Include(x => x.StayingWith)
                    .Include(x => x.BloodType)
                    .Include(z => z.SiblingGroup)
                    .Include(x => x.StudentStatus)
                    .Where(x => /*siblingGroups.Contains(x.Id)*/
                                x.Id == id)
                    .ToListAsync(CancellationToken);

            var getStudentPhoto = await _dbStudentContext.Entity<TrStudentPhoto>()
                        .Where(x => x.IdStudent == id)
                        .Where(x => x.IdAcademicYear == acadYear.Id)
                        .FirstOrDefaultAsync(CancellationToken);

            var retVal = query
                .Select(x => new GetStudentDetailResult
                {
                    SiblingGroup = siblingGroups == null ? null : siblingGroups,
                    //ParentGroup = parentGroups == null ? null : parentGroups,

                    ParentGroup = x.StudentParents.Count() > 0
                        ? x.StudentParents.Select(x => new ParentGroupDetailVm(x.Parent.Id, x.Parent.ParentRole.ParentRoleNameEng)).ToList()
                        : null,
                    Id = x.Id,
                    Description = "Student",
                    NameInfo = new NameInfoVm
                    {
                        FirstName = x.LastName.Contains(" ") == true ? x.FirstName + " " + x.LastName.Substring(0, x.LastName.LastIndexOf(' ')).TrimEnd() : x.FirstName?.Trim(),
                        //MiddleName = x.MiddleName,
                        LastName = x.LastName.Contains(" ") == true ? x.LastName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last() : x.LastName.Trim()
                    },
                    PersonalInfoVm = new PersonalStudentInfoDetailVm
                    {
                        Photo = getStudentPhoto != null ? getStudentPhoto.FilePath : (studentHomeroomDetail == null ? null : GetStudentPhoto(school == null ? null : school.Description, acadYear.Id, x.Id, container)),
                        //Photo = getStudentPhoto == null ? null : getStudentPhoto.FilePath /*GetStudentPhoto(school == null ? null : school.Description, acadYear.Id, x.Id, container)*/,
                        Grade = gradeResult.Description ?? gradeResult.Description,
                        Class = ClassResult.Description ?? ClassResult.Description,
                        IdStudentStatus = x.StudentStatus.IdStudentStatus,
                        StudentStatus = x.StudentStatus.LongDesc,
                        SchoolLevel = schoolLevel == null ? null : schoolLevel.Description,
                        ChildNumber = x.ChildNumber,
                        TotalChildInFamily = x.TotalChildInFamily,
                        IdChildStatus = x.ChildStatus == null ? null : new ItemValueVm
                        {
                            Id = x.ChildStatus.Id,
                            Description = x.ChildStatus.ChildStatusName
                        },
                    },
                    IdInfo = new IdInfoDetailVm
                    {
                        IdSiblingGroup = siblingGroupId == null ? null : siblingGroupId,
                        IdRegistrant = x.IdRegistrant,
                        IdSchool = schoolInfo,
                        IdBinusian = x.IdBinusian,
                        NISN = x.NISN
                    },
                    BirthInfo = new BirthInfoDetailVm
                    {
                        POB = x.POB,
                        DOB = x.DOB,
                        IdBirthCountry = birthCountryInfo,
                        IdBirthStateProvince = birthProvinceInfo,
                        IdBirthCity = birthCityInfo,
                        IdNationality = x.Nationality == null ? null : new ItemValueVm
                        {
                            Id = x.Nationality.Id,
                            Description = x.Nationality.NationalityName
                        },
                        IdCountry = x.Country == null ? null : new ItemValueVm
                        {
                            Id = x.Country.Id,
                            Description = x.Country.CountryName
                        }
                    },
                    ReligionInfo = x.Religion == null ? null : new ReligionInfoVm
                    {
                        IdReligion = new ItemValueVm
                        {
                            Id = x.Religion.Id,
                            Description = x.Religion.ReligionName
                        },
                        IdReligionSubject = x.ReligionSubject == null ? null : new ItemValueVm
                        {
                            Id = x.ReligionSubject.Id,
                            Description = x.ReligionSubject.ReligionSubjectName
                        }
                    },
                    AddressInfo = new AddressInfoDetailVm
                    {
                        IdStayingWith = x.StayingWith == null ? null : new ItemValueVm
                        {
                            Id = x.StayingWith.IdStayingWith.ToString(),
                            Description = x.StayingWith.StayingWithName
                        },
                        ResidenceAddress = x.ResidenceAddress,
                        HouseNumber = x.HouseNumber,
                        RT = x.RT,
                        RW = x.RW,
                        VillageDistrict = x.VillageDistrict,
                        SubDistrict = x.SubDistrict,
                        IdAddressCity = addressCityInfo,
                        IdAddressStateProvince = addressProvinceInfo,
                        IdAddressCountry = addressCountryInfo,
                        PostalCode = x.PostalCode,
                        DistanceHomeToSchool = ((decimal)(x.DistanceHomeToSchool != null ? x.DistanceHomeToSchool : 0))
                    },
                    CardInfo = new CardInfoVm
                    {
                        FamilyCardNumber = x.FamilyCardNumber,
                        NIK = x.NIK,
                        KITASNumber = x.KITASNumber,
                        KITASExpDate = x.KITASExpDate.HasValue ? x.KITASExpDate : null,
                        NSIBNumber = x.NSIBNumber,
                        NSIBExpDate = x.NSIBExpDate.HasValue ? x.NSIBExpDate : null,
                        PassportNumber = x.PassportNumber,
                        PassportExpDate = x.PassportExpDate.HasValue ? x.PassportExpDate : null,
                        IsHavingKJP = x.IsHavingKJP
                    },
                    PreviousSchoolInfo = prevSchoolNew,
                    ContactInfo = new ContactInfoDetailVm
                    {
                        ResidencePhoneNumber = x.ResidencePhoneNumber,
                        MobilePhoneNumber1 = x.MobilePhoneNumber1,
                        MobilePhoneNumber2 = x.MobilePhoneNumber2,
                        MobilePhoneNumber3 = x.MobilePhoneNumber3,
                        /*IDEmergencyContactRole = new ItemValueVm
                        {
                            Id = emergencyContactRole.Id,
                            Description = emergencyContactRole.ParentRoleNameEng
                        },*/
                        EmergencyContactRole = emergencyContactRoleDetail,
                        //ListEmergencyContactRole = x.StudentParents.Count() > 0 ? x.StudentParents.Select(x =>x.Parent.ParentRole.ParentRoleNameEng).First(): null,
                        /*ListEmergencyContactRole = x.StudentParents.Count() > 0 ? x.StudentParents.Select(x =>
                            new EmergencyContactInfoVm
                            {
                                EmergencyContact = x.Parent.ParentRole.ParentRoleNameEng,
                                EmergencyContactName = x.Parent.FirstName,
                                EmergencyContactNumber = x.Parent.MobilePhoneNumber1,
                                EmergencyEmailNumber = x.Parent.PersonalEmailAddress
                            }).ToList() : new List<EmergencyContactInfoVm>(),*/
                        BinusianEmailAddress = x.BinusianEmailAddress,
                        PersonalEmailAddress = x.PersonalEmailAddress
                    },
                    MedicalInfo = new MedicalInfoVm
                    {
                        Gender = x.Gender,
                        IdBloodType = new ItemValueVm
                        {
                            Id = x.BloodType.Id,
                            Description = x.BloodType.BloodTypeName
                        },
                        Height = (int)(x.Height != null ? x.Height : 0),
                        Weight = (int)(x.Weight != null ? x.Weight : 0)
                    },
                    BankAndVAInfo = new BankAndVAInfoDetailVm
                    {
                        BankName = bankNameInfo,
                        BankAccountNumber = bankVAAccount == null ? null : bankVAAccount.AccountNumberCurrentValue,
                        BankAccountRecipient = bankVAAccount == null ? null : bankVAAccount.AccountNameCurrentValue,
                        SchoolVAName = x.SchoolVAName,
                        SchoolVANumber = x.SchoolVANumber

                    },
                    OccupationInfo = x.StudentParents.Count() > 0 ? x.StudentParents.Select(x =>
                        new OccupationInfoVm
                        {
                            ParentRole = x.Parent.ParentRole == null ? null : new ItemValueVm
                            {
                                Id = x.Parent.ParentRole.Id,
                                Description = x.Parent.ParentRole.ParentRoleNameEng
                            },
                            CompanyNama = x.Parent.CompanyName,
                            JobPosition = x.Parent.OccupationPosition,
                            IdOccupationType = x.Parent.OccupationType == null ? null : new ItemValueVm
                            {
                                Id = x.Parent.OccupationType.Id,
                                Description = x.Parent.OccupationType.OccupationTypeNameEng
                            },
                            IdParentSalaryGroup = x.Parent.ParentSalaryGroup == null ? null : new ItemValueVm
                            {
                                Id = x.Parent.ParentSalaryGroup.IdParentSalaryGroup.ToString(),
                                Description = x.Parent.ParentSalaryGroup.ParentSalaryGroupName
                            }
                        }).ToList() : null,
                    SpecialTreatmentInfo = new SpecialTreatmentInfoVm
                    {
                        IsSpecialTreatment = x.IsSpecialTreatment,
                        NotesForSpecialTreatments = x.NotesForSpecialTreatments
                    },
                    AdditionalInfo = new AdditionalInfoVm
                    {
                        FutureDream = x.FutureDream,
                        Hobby = x.Hobby
                    },

                    Audit = x.GetRawAuditResult2()
                })
                .FirstOrDefault();

            return Request.CreateApiResult2(retVal as object);
        }
        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<CollectionRequest>(nameof(CollectionSchoolRequest.IdSchool));
            var predicate = PredicateBuilder.Create<MsStudent>(x => true);

            var query = _dbStudentContext.Entity<MsStudent>()
                .Where(predicate)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, $"{x.FirstName} {x.LastName}"))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .Include(x => x.StudentParents).ThenInclude(p => p.Parent)
                    .Include(x => x.Country)
                    .Include(x => x.Religion)
                    .Include(x => x.ReligionSubject)
                    .Include(x => x.BloodType)
                    /*.Include(x => x.Province)
                    .Include(x => x.City)
                    .Include(x => x.Nationality)
                    */
                    .Select(x => new GetStudentResult
                    {
                        Id = x.Id,
                        Description = "Student",
                        nameInfo = new NameInfoVm
                        {
                            FirstName = x.LastName.Contains(" ") == true ? x.FirstName + " " + x.LastName.Substring(0, x.LastName.LastIndexOf(' ')).TrimEnd() : x.FirstName.Trim(),
                            //MiddleName = x.MiddleName,
                            LastName = x.LastName.Contains(" ") == true ? x.LastName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last() : x.LastName.Trim()
                        },
                        idInfo = new IdInfoVm
                        {
                            IdRegistrant = x.IdRegistrant,
                            IdSchool = x.IdSchool,
                            IdBinusian = x.IdBinusian,
                            NISN = x.NISN
                        },
                        birthInfo = new BirthInfoVm
                        {
                            POB = x.POB,
                            DOB = x.DOB,
                            IdBirthCountry = x.IdBirthCountry,
                            IdBirthStateProvince = x.IdBirthStateProvince,
                            IdBirthCity = x.IdBirthCity,
                            /*IdNationality = x.IdNationality,
                            IdCountry = x.IdCountry*/
                            /*IdBirthCountry = new ItemValueVm
                            {
                                Id = x.BirthCountry.Id,
                                Description = x.BirthCountry.CountryName
                            },
                            IdBirthStateProvince = new ItemValueVm
                            {
                                Id = x.Province.Id,
                                Description = x.Province.ProvinceName
                            },
                            IdBirthCity = new ItemValueVm
                            {
                                Id = x.City.Id,
                                Description = x.City.CityName
                            },*/
                            IdNationality = x.Nationality == null ? null : new ItemValueVm
                            {
                                Id = x.Nationality.Id,
                                Description = x.Nationality.NationalityName
                            },
                            IdCountry = x.Country == null ? null : new ItemValueVm
                            {
                                Id = x.Country.Id,
                                Description = x.Country.CountryName
                            }
                        },
                        religionInfo = new ReligionInfoVm
                        {
                            /*IdReligion = x.IdReligion,
                            IdReligionSubject = x.IdReligionSubject*/
                            IdReligion = x.Religion == null ? null : new ItemValueVm
                            {
                                Id = x.Religion.Id,
                                Description = x.Religion.ReligionName
                            },
                            IdReligionSubject = x.ReligionSubject == null ? null : new ItemValueVm
                            {
                                Id = x.ReligionSubject.Id,
                                Description = x.ReligionSubject.ReligionSubjectName
                            }
                        },
                        addressInfo = new AddressInfoVm
                        {
                            IdStayingWith = x.IdStayingWith,
                            ResidenceAddress = x.ResidenceAddress,
                            HouseNumber = x.HouseNumber,
                            RT = x.RT,
                            RW = x.RW,
                            VillageDistrict = x.VillageDistrict,
                            SubDistrict = x.SubDistrict,
                            IdAddressCity = x.IdAddressCity,
                            IdAddressStateProvince = x.IdAddressStateProvince,
                            IdAddressCountry = x.IdAddressCountry,
                            PostalCode = x.PostalCode,
                            DistanceHomeToSchool = ((decimal)(x.DistanceHomeToSchool != null ? x.DistanceHomeToSchool : 0))
                        },
                        cardInfo = new CardInfoVm
                        {
                            FamilyCardNumber = x.FamilyCardNumber,
                            NIK = x.NIK,
                            KITASNumber = x.KITASNumber,
                            KITASExpDate = x.KITASExpDate.HasValue ? x.KITASExpDate : null,
                            NSIBNumber = x.NSIBNumber,
                            NSIBExpDate = x.NSIBExpDate.HasValue ? x.NSIBExpDate : null,
                            PassportNumber = x.PassportNumber,
                            PassportExpDate = x.PassportExpDate.HasValue ? x.PassportExpDate : null
                        },
                        contactInfo = new ContactInfoVm
                        {
                            ResidencePhoneNumber = x.ResidencePhoneNumber,
                            MobilePhoneNumber1 = x.MobilePhoneNumber1,
                            MobilePhoneNumber2 = x.MobilePhoneNumber2,
                            MobilePhoneNumber3 = x.MobilePhoneNumber3,
                            EmergencyContactRole = x.EmergencyContactRole,
                            BinusianEmailAddress = x.BinusianEmailAddress,
                            PersonalEmailAddress = x.PersonalEmailAddress
                        },
                        medicalInfo = new MedicalInfoVm
                        {
                            Gender = x.Gender,
                            IdBloodType = x.BloodType == null ? null : new ItemValueVm
                            {
                                Id = x.BloodType.Id,
                                Description = x.BloodType.BloodTypeName
                            },
                            Height = (int)(x.Height != null ? x.Height : 0),
                            Weight = (int)(x.Weight != null ? x.Weight : 0)
                        },
                        occupationInfo = x.StudentParents.Count() > 0 ? x.StudentParents.Select(x =>
                            new OccupationInfoVm
                            {
                                CompanyNama = x.Parent.CompanyName,
                                JobPosition = x.Parent.OccupationPosition,
                                IdOccupationType = x.Parent.OccupationType == null ? null : new ItemValueVm
                                {
                                    Id = x.Parent.OccupationType.Id,
                                    Description = x.Parent.OccupationType.OccupationTypeNameEng
                                },
                                IdParentSalaryGroup = x.Parent.ParentSalaryGroup == null ? null : new ItemValueVm
                                {
                                    Id = x.Parent.ParentSalaryGroup.IdParentSalaryGroup.ToString(),
                                    Description = x.Parent.ParentSalaryGroup.ParentSalaryGroupName
                                }
                            }).FirstOrDefault() : null,
                        specialTreatmentInfo = new SpecialTreatmentInfoVm
                        {
                            IsSpecialTreatment = x.IsSpecialTreatment,
                            NotesForSpecialTreatments = x.NotesForSpecialTreatments
                        },
                        additionalInfo = new AdditionalInfoVm
                        {
                            FutureDream = x.FutureDream,
                            Hobby = x.Hobby
                        }
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }
        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }
        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            try
            {
                var body = await Request.ValidateBody<UpdateStudentRequest, UpdateStudentValidator>();
                _transaction = await _dbStudentContext.BeginTransactionAsync(CancellationToken);

                var getdata = await _dbStudentContext.Entity<MsStudent>().Where(p => p.Id == body.IdStudent).FirstOrDefaultAsync();
                if (getdata is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Student ID"], "Id", body.IdStudent));

                #region Old Validation
                /*var birthCountry = await _dbStudentContext.Entity<LtCountry>().FindAsync(body.IdBirthCountry);
                if (birthCountry is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Birth Country"], "Id", body.IdBirthCountry));

                var birthStateProvince = await _dbStudentContext.Entity<LtDistrict>().FindAsync(body.IdBirthStateProvince);
                if (birthStateProvince is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Birth State Province"], "Id", body.IdBirthStateProvince));
                
                var birthCity = await _dbStudentContext.Entity<LtCity>().FindAsync(body.IdBirthCity);
                if (birthCity is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Birth City"], "Id", body.IdBirthCity));

                var nationality = await _dbStudentContext.Entity<LtNationality>().FindAsync(body.IdNationality);
                if (nationality is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Nationality"], "Id", body.IdNationality));

                var nationalityCountry = await _dbStudentContext.Entity<MsNationalityCountry>().FindAsync(body.IdCountry);
                if (nationalityCountry is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Country"], "Id", body.IdCountry));

                var religion = await _dbStudentContext.Entity<LtReligion>().FindAsync(body.IdReligion);
                if (religion is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Religion"], "Id", body.IdReligion));

                var religionSubject = await _dbStudentContext.Entity<LtReligionSubject>().FindAsync(body.IdReligionSubject);
                if (religionSubject is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Religion Subject"], "Id", body.IdReligionSubject));

                var childStatus = await _dbStudentContext.Entity<LtChildStatus>().FindAsync(body.IdChildStatus);
                if (childStatus is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Child Status"], "Id", body.IdChildStatus));

                var bloodType = await _dbStudentContext.Entity<LtBloodType>().FindAsync(body.IdBloodType);
                if (bloodType is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Blood Type"], "Id", body.IdBloodType));

                var stayingWith = await _dbStudentContext.Entity<LtParentRole>().FindAsync(body.IdStayingWith);
                if (stayingWith is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Staying With"], "Id", body.IdStayingWith));
                
                var addressCity = await _dbStudentContext.Entity<LtCity>().FindAsync(body.IdAddressCity);
                if (addressCity is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Address City"], "Id", body.IdAddressCity));

                var addressStateProvince = await _dbStudentContext.Entity<LtProvince>().FindAsync(body.IdAddressStateProvince);
                if (addressStateProvince is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Address State Province"], "Id", body.IdAddressStateProvince));

                var addressCountry = await _dbStudentContext.Entity<LtCountry>().FindAsync(body.IdAddressCountry);
                if (addressCountry is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Address Country"], "Id", body.IdAddressCountry));

                var studentStatus = await _dbStudentContext.Entity<ltst>().FindAsync(body.IdStayingWith);
                if (studentStatus is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Student Status"], "Id", body.IdStayingWith));
                */
                #endregion

                body.ChildNumber = body.ChildNumber == 0 ? getdata.ChildNumber : body.ChildNumber;
                body.TotalChildInFamily = body.TotalChildInFamily == 0 ? getdata.TotalChildInFamily : body.TotalChildInFamily;

                foreach (var prop in body.GetType().GetProperties())
                {
                    //var oldVal = getdata.GetType().GetProperty(prop.Name).GetValue(getdata, null);
                    var newVal = body.GetType().GetProperty(prop.Name).GetValue(body, null);

                    Type myType = getdata.GetType();
                    PropertyInfo pinfo = myType.GetProperty(prop.Name);
                    //var newVal = body.GetType().GetProperty(prop.Name).GetValue(body, null);
                    if (pinfo != null && newVal != null)
                    {

                        pinfo.SetValue(getdata, newVal);
                        //var propertyInfo = getdata.GetType().GetProperty(prop.Name);
                        //propertyInfo.SetValue(getdata, newVal);
                        //getdata.GetType().GetProperty(prop.Name).SetValue(getdata, newVal);
                    }
                }

                _dbStudentContext.Entity<MsStudent>().Update(getdata);

                await _dbStudentContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
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
        }
    }
}
