using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.EmployeeDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Employee.FnStaff.Teacher;
using System.IO;
using System.Text.Json;
using System.Text;
using BinusSchool.Common.Model.Enums;
using Microsoft.Extensions.Configuration;
using BinusSchool.Data.Configurations;

namespace BinusSchool.Employee.FnStaff.Teacher
{
    public class TeacherHandler : FunctionsHttpCrudHandler
    {
        private readonly IEmployeeDbContext _dbContext;
        private readonly ICountry _serviceCountry;
        private readonly IUser _serviceUser;
        private readonly string _keyValue;
        private readonly IConfiguration _configuration;
        public TeacherHandler(IEmployeeDbContext dbContext,
            IUser serviceUser,
            ICountry serviceCountry,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _serviceCountry = serviceCountry;
            _serviceUser = serviceUser;
            _keyValue = configuration["keyValue"];
            _configuration = configuration;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public static string GetApi(string ApiUrl, string keyValue)
        {

            var responseString = "";
            var request = (HttpWebRequest)WebRequest.Create(ApiUrl);

            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", keyValue);
            try
            {
                using (var response1 = request.GetResponse())
                {
                    using (var reader = new StreamReader(response1.GetResponseStream()))
                    {
                        responseString = reader.ReadToEnd();
                    }
                }
                return responseString;
            }
            catch (WebException e)
            {
                return responseString = "{}";
            }
        }

        public static string PostApi(string ApiUrl, string keyValue, string IdBinusian)
        {
            var responseString = "";
            string json = JsonSerializer.Serialize(new
            {
                IdBinusian = IdBinusian
            });
            var request = (HttpWebRequest)WebRequest.Create(ApiUrl);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", keyValue);
            request.ContentLength = json.Length;

            using (var streamwriter = new StreamWriter(request.GetRequestStream()))
            {
                streamwriter.Write(json);
            }
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            catch (WebException e)
            {
                return responseString = "{}";
            }
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var apiConfig = _configuration.GetSection("BinusianService").Get<ApiConfiguration>();

            var responseString = GetApi(apiConfig.Host.Trim().ToString() + "/binusschool/auth/token", _keyValue);
            Token keyToken = JsonSerializer.Deserialize<Token>(responseString);
            string IdBinusian = id;
            var dataStaff = PostApi(apiConfig.Host.Trim().ToString() + "/binusschool/staffinformation", keyToken.errorMessage == null ? "" : "Bearer " + keyToken.data.token, IdBinusian);
            DataStaff staffRespond = JsonSerializer.Deserialize<DataStaff>(dataStaff);

            //var photoStaff = PostApi(apiConfig.Host.Trim().ToString() + "/binusschool/binusianphoto", keyToken.errorMessage == null ? "" : "Bearer " + /*keyToken.data.token*/, IdBinusian);
            //DataStaff photoStaffRespond = null;//JsonSerializer.Deserialize<DataStaff>(dataStaff);

            if (staffRespond.errorMessage is null)
                staffRespond = null;

            var jobData = _dbContext.Entity<MsStaffJobInformation>()
                            .Where(x => x.IdBinusian == id)
                            .Select(x => new {
                                x.IdEmployeeStatus,
                                x.IdPTKType,
                                x.IdExpSpecialTreatments,
                                x.IdLabSkillsLevel,
                                x.IdIsyaratLevel,
                                x.IdBrailleExpLevel
                            }).FirstOrDefault();

            var EmployeeStatusData = _dbContext.Entity<LtEmployeeStatus>()
                                .Where(x => x.Id == (jobData == null ? null : jobData.IdEmployeeStatus))
                                .Select(x => new { id = x.Id, description = x.EmployeeStatusDesc })
                                .FirstOrDefault();

            var PTKTypeData = _dbContext.Entity<LtPTKType>()
                                .Where(x => x.Id == (jobData == null ? null : jobData.IdPTKType))
                                .Select(x => new { id = x.Id, description = x.PTKTypeEngName })
                                .FirstOrDefault();
            var SpecialTreatmentsSkillsLevelData = _dbContext.Entity<LtSpecialTreatmentsSkillsLevel>()
                                .Where(x => x.IdExpSpecialTreatments == (jobData == null ? null : jobData.IdExpSpecialTreatments))
                                .Select(x => new { id = x.IdExpSpecialTreatments, description = x.ExpSpecialTreatmentsEngName })
                                .FirstOrDefault();
            var LabSkillsLevelData = _dbContext.Entity<LtLabSkillsLevel>()
                                .Where(x => x.Id == (jobData == null ? null : jobData.IdLabSkillsLevel))
                                .Select(x => new { id = x.Id, description = x.LabSkillsLevelEngName })
                                .FirstOrDefault();
            var IsyaratLevelData = _dbContext.Entity<LtIsyaratLevel>()
                                .Where(x => x.Id == (jobData == null ? null : jobData.IdIsyaratLevel))
                                .Select(x => new { id = x.Id, description = x.IsyaratLevelDescEngName })
                                .FirstOrDefault();
            var BrailleExpLevelData = _dbContext.Entity<LtBrailleExpLevel>()
                                .Where(x => x.Id == (jobData == null ? null : jobData.IdBrailleExpLevel))
                                .Select(x => new { id = x.Id, description = x.BrailleExpDescEngName })
                                .FirstOrDefault();

            var EmployeeStatusDataInfo = EmployeeStatusData != null ? new ItemValueVm
            {
                Id = EmployeeStatusData.id,
                Description = EmployeeStatusData.description
            } : null;
            var PTKTypeDataInfo = PTKTypeData != null ? new ItemValueVm
            {
                Id = PTKTypeData.id,
                Description = PTKTypeData.description
            } : null;
            var SpecialTreatmentsSkillsLevelDataInfo = SpecialTreatmentsSkillsLevelData != null ? new ItemValueVm
            {
                Id = SpecialTreatmentsSkillsLevelData.id,
                Description = SpecialTreatmentsSkillsLevelData.description
            } : null;
            var LabSkillsLevelDataInfo = LabSkillsLevelData != null ? new ItemValueVm
            {
                Id = LabSkillsLevelData.id,
                Description = LabSkillsLevelData.description
            } : null;
            var IsyaratLevelDataInfo = IsyaratLevelData != null ? new ItemValueVm
            {
                Id = IsyaratLevelData.id,
                Description = IsyaratLevelData.description
            } : null;
            var BrailleExpLevelDataInfo = BrailleExpLevelData != null ? new ItemValueVm
            {
                Id = BrailleExpLevelData.id,
                Description = BrailleExpLevelData.description
            } : null;

            var staffJobinfo = _dbContext.Entity<MsStaffJobInformation>()
                                .Where(x => x.IdBinusian == id)
                                .Select(x => new JobInfoDetailVm
                                {
                                    IdBusinessUnit = x.IdBusinessUnit,
                                    BusinessUnitName = x.BusinessUnitName,
                                    IdDepartment = x.IdDepartment,
                                    DepartmentName = x.DepartmentName,
                                    IdPosition = x.IdPosition,
                                    PositionName = x.PositionName,
                                    SubjectSpecialization = x.SubjectSpecialization,
                                    TeacherDurationWeek = x.TeacherDurationWeek,
                                    NUPTK = x.NUPTK,
                                    IdEmployeeStatus = EmployeeStatusDataInfo,
                                    IdPTKType = PTKTypeDataInfo,
                                    NoSrtPengangkatan = x.NoSrtPengangkatan,
                                    TglSrtPengangkatan = x.TglSrtPengangkatan,
                                    NoSrtKontrak = x.NoSrtKontrak,
                                    TglSrtKontrakKerja = x.TglSrtKontrakKerja,
                                    NoIndukGuruKontrak = x.NoIndukGuruKontrak,
                                    IsPrincipalLicensed = x.IsPrincipalLicensed,
                                    IdExpSpecialTreatments = SpecialTreatmentsSkillsLevelDataInfo,
                                    IdLabSkillsLevel = LabSkillsLevelDataInfo,
                                    IdIsyaratLevel = IsyaratLevelDataInfo,
                                    IdBrailleExpLevel = BrailleExpLevelDataInfo,
                                    AdditionalTaskNotes = x.AdditionalTaskNotes,
                                    Division = x.Division,
                                    IdBusinessUnitGroup = x.IdBusinessUnitGroup,
                                    BusinessUnitGroupName = x.BusinessUnitGroupName
                                })
                                .FirstOrDefault();

            var staffFamilyinfo = _dbContext.Entity<TrStaffFamilyInformation>()
                                .Where(x => x.IdBinusian == id)
                                .Select(x => new FamilyInfoDetailVm
                                {
                                    FamilyName = x.FamilyName,
                                    RelationshipStatus = x.RelationshipStatus
                                })
                                .ToList();

            string genderName = _dbContext.Entity<MsStaff>().Where(x => x.IdBinusian == id).Select(x => x.GenderName).FirstOrDefault();
            Gender enumGender = (Gender)Enum.Parse(typeof(Gender), genderName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last());

            var query = await _dbContext.Entity<MsStaff>()
                .Include(x => x.StaffFamilyInformation)
                .Include(x => x.KITASStatus)
                .Include(x => x.IMTASchoolLevel)
                .Include(x => x.IMTAMajorAssignPosition)
                .Where(x => x.IdBinusian == id)
                .Select(x => new GetTeacherDetailResult
                {
                    PersonalInfo = new PersonalInfoDetailVM
                    {
                        Photo = null,
                        IdDesignation = new ItemValueVm
                        {
                            Id = x.IdDesignation.ToString(),
                            Description = x.Designation.DesignationDescription
                        },
                        IdEmployee = staffRespond == null ? x.IdEmployee : staffRespond.staffInformationResponse.nip,
                        IdBinusian = staffRespond == null ? x.IdBinusian : staffRespond.staffInformationResponse.binusian_id,
                        FirstName = staffRespond == null ? x.FirstName : (staffRespond.staffInformationResponse.nama.Contains(" ") ? staffRespond.staffInformationResponse.nama.Substring(0, staffRespond.staffInformationResponse.nama.LastIndexOf(' ')).TrimEnd() : staffRespond.staffInformationResponse.nama),
                        LastName = staffRespond == null ? x.LastName : (staffRespond.staffInformationResponse.nama.Contains(" ") ? staffRespond.staffInformationResponse.nama.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last() : staffRespond.staffInformationResponse.nama),
                        POB = staffRespond == null ? x.POB : staffRespond.staffInformationResponse.tmp_lahir,
                        DOB = staffRespond == null ? new DateTime() : staffRespond.staffInformationResponse.tgl_lahir,
                        GenderName = staffRespond == null ? enumGender : (Gender)Enum.Parse(typeof(Gender), staffRespond.staffInformationResponse.nmkel.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last()),
                        MotherMaidenName = staffRespond == null ? x.MotherMaidenName : staffRespond.staffInformationResponse.nama_ibu,
                        IdNationality = new ItemValueVm
                        {
                            Id = staffRespond == null ? x.IdNationality : staffRespond.staffInformationResponse.kd_kewarganegaraan,
                            Description = staffRespond == null ? "" : staffRespond.staffInformationResponse.nm_Kewarganegaraan
                        },
                        IdNationalityCountry = new ItemValueVm
                        {
                            Id = staffRespond == null ? x.IdNationalityCountry : staffRespond.staffInformationResponse.negara,
                            Description = staffRespond == null ? "" : staffRespond.staffInformationResponse.nmneg,
                        },

                        IdReligion = new ItemValueVm
                        {
                            Id = staffRespond == null ? x.IdReligion : staffRespond.staffInformationResponse.kd_agama,
                            Description = staffRespond == null ? "" : staffRespond.staffInformationResponse.nmagm
                        },
                        NIK = staffRespond == null ? x.NIK : staffRespond.staffInformationResponse.nik,
                        PassportNumber = staffRespond == null ? x.PassportNumber : staffRespond.staffInformationResponse.passportNumber,
                        PassportExpDate = staffRespond == null ? new DateTime() : staffRespond.staffInformationResponse.passportExpiredDate,
                        MaritalStatus = staffRespond == null ? x.MaritalStatus.ToString() : staffRespond.staffInformationResponse.status_pernikahan
                    },
                    FamilyInfo = staffFamilyinfo == null ? null : staffFamilyinfo,//: staffRespond.staffInformationResponse.familyMemberList,
                    ExpatriateInfo = new ExpatriateInfoDetailVm
                    {
                        KITASNumber = x.KITASNumber,
                        KITASSponsor = x.KITASSponsor,
                        IdKITASStatus = new ItemValueVm
                        {
                            Id = x.IdKITASStatus,
                            Description = x.KITASStatus.KITASStatusDescription
                        },
                        KITASExpDate = x.KITASExpDate,
                        IMTANumber = x.IMTANumber,
                        IdIMTASchoolLevel = new ItemValueVm
                        {
                            Id = x.IdIMTASchoolLevel,
                            Description = x.IMTASchoolLevel.IMTASchoolLevelEngName
                        },
                        IdIMTAMajorAssignPosition = new ItemValueVm
                        {
                            Id = x.IdIMTAMajorAssignPosition,
                            Description = x.IMTAMajorAssignPosition.IMTAMajorAssignPosition
                        },
                        IMTAExpDate = x.IMTAExpDate
                    },
                    AddressInfo = new AddressInfoDetailVm
                    {
                        ResisdenceAddress = staffRespond == null ? x.ResidenceAddress : staffRespond.staffInformationResponse.alamat,
                        AddressCity = staffRespond == null ? x.AddressCity : staffRespond.staffInformationResponse.kota,
                        PostalCode = staffRespond == null ? x.PostalCode : staffRespond.staffInformationResponse.kdPos,
                    },
                    ContactInfo = new ContactInfoDetailVm
                    {
                        ResidencePhoneNumber = staffRespond == null ? x.ResidencePhoneNumber : staffRespond.staffInformationResponse.residencePhoneNumber,
                        MobilePhoneNumber1 = staffRespond == null ? x.MobilePhoneNumber1 : staffRespond.staffInformationResponse.mobilePhoneNumber,
                        BinusianEmailAddress = staffRespond == null ? x.BinusianEmailAddress : staffRespond.staffInformationResponse.email1,
                        PersonalEmailAddress = staffRespond == null ? x.PersonalEmailAddress : staffRespond.staffInformationResponse.email2,
                    },
                    JobInfo = staffJobinfo == null ? null : staffJobinfo,
                })
                .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }
        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }
        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }
        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
