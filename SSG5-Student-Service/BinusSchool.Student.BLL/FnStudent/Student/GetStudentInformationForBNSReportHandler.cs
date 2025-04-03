using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Student.FnStudent.Student.Validator;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetStudentInformationForBNSReportHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _pendidikanTK = new[] { "1", "2", "3" };
        private static readonly string[] _pendidikanSD = new[] { "1", "2", "3", "4", "5", "6" };
        private static readonly string[] _pendidikanSMP = new[] { "7", "8", "9" };
        private static readonly string[] _pendidikanSMA = new[] { "10", "11", "12" };

        private readonly IStudentDbContext _dbContext;

        public GetStudentInformationForBNSReportHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetStudentInformationForBNSReportRequest, GetStudentInformationForBNSReportValidator>();

            var result = await GetStudentInformationForBNSReport(new GetStudentInformationForBNSReportRequest
            {
                IdStudent = param.IdStudent,
                IdGrade = param.IdGrade,
                Semester = param.Semester,
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<GetStudentInformationForBNSReportResult> GetStudentInformationForBNSReport(GetStudentInformationForBNSReportRequest param)
        {
            var getStudentData = await GetStudentData(param);
            var getParentData = await GetParentData(param);

            var retVal = new GetStudentInformationForBNSReportResult()
            {
                Student = getStudentData,
                Parent = getParentData,
            };

            return retVal;
        }

        public async Task<GetStudentInformationForBNSReportResult_Student> GetStudentData(GetStudentInformationForBNSReportRequest param)
        {
            var getStudentDataDetail = _dbContext.Entity<MsHomeroomStudent>()
                    .Where(x => x.Semester == param.Semester &&
                                x.IdStudent == param.IdStudent &&
                                x.Homeroom.IdGrade == param.IdGrade)
                    .Select(x => new
                    {
                        IdBinusian = x.Student.IdBinusian,
                        IdStudent = x.IdStudent,
                        StudentName = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                        NISN = x.Student.NISN ?? "-",
                        POB = x.Student.POB,
                        DOB = x.Student.DOB != null ? x.Student.DOB.Value.ToString("dd MMMM yyyy") : "",
                        DOBIndo = x.Student.DOB != null ? x.Student.DOB.Value.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID")) : "",
                        ChildNumber = x.Student.ChildNumber,
                        Telephone = x.Student.MobilePhoneNumber1,
                        Gender = x.Student.Gender,
                        Religion = x.Student.Religion.ReligionName,
                        ReligionSubject = x.Student.ReligionSubject.ReligionSubjectName,
                        ChildStatus = x.Student.ChildStatus.ChildStatusName,
                        ResidenceAddress = x.Student.ResidenceAddress,
                        SchoolId = x.Homeroom.Grade.MsLevel.MsAcademicYear.MsSchool.Id,
                        SchoolName = x.Homeroom.Grade.MsLevel.MsAcademicYear.MsSchool.Name,
                        AcademicYearId = x.Homeroom.Grade.MsLevel.IdAcademicYear,
                        AcademicYearCode = x.Homeroom.Grade.MsLevel.MsAcademicYear.Code,
                        AcademicYearDescription = x.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                        LevelId = x.Homeroom.Grade.IdLevel,
                        LevelCode = x.Homeroom.Grade.MsLevel.Code,
                        LevelDescription = x.Homeroom.Grade.MsLevel.Description,
                        GradeId = x.Homeroom.IdGrade,
                        GradeCode = x.Homeroom.Grade.Code,
                        GradeDescription = x.Homeroom.Grade.Description,
                        HomeroomId = x.IdHomeroom,
                        HomeroomCode = x.Homeroom.Grade.Code + "" + Regex.Replace(x.Homeroom.MsGradePathwayClassroom.Classroom.Code, @"[^a-zA-Z0-9]", string.Empty),
                        HomeroomDescription = x.Homeroom.Grade.Code + "" + Regex.Replace(x.Homeroom.MsGradePathwayClassroom.Classroom.Code, @"[^a-zA-Z0-9]", string.Empty),
                        Semester = param.Semester
                    }).Distinct()
                    .FirstOrDefault();

            var getDataStudentAllEnrollment = await GetDataStudentAllEnrollment(param, getStudentDataDetail.AcademicYearCode);
            var getStudentPrevSchool = await GetStudentPrevSchool(param, getDataStudentAllEnrollment);
            var getEntrySchoolByLevel = GetEntrySchoolByLevel(param, getDataStudentAllEnrollment);
            var getJoinToSchoolDate = await GetJoinToSchoolDate(param, getDataStudentAllEnrollment);
            var getStudentPhoto = await GetStudentPhoto(getStudentDataDetail.AcademicYearCode, param.IdStudent);

            var getStudentData = new GetStudentInformationForBNSReportResult_Student()
            {
                StudentDetail = new GetStudentInformationForBNSReportResult_StudentDetail()
                {
                    IdBinusian = getStudentDataDetail.IdBinusian?.Trim(),
                    IdStudent = getStudentDataDetail.IdStudent,
                    StudentName = getStudentDataDetail.StudentName,
                    NISN = getStudentDataDetail.NISN,
                    POB = getStudentDataDetail.POB,
                    DOB = getStudentDataDetail.DOB,
                    DOBIndo = getStudentDataDetail.DOBIndo,
                    Telephone = getStudentDataDetail.Telephone?.Trim(),
                    ChildNumber = getStudentDataDetail.ChildNumber != 0 ? getStudentDataDetail.ChildNumber.ToString() : "",
                    Gender = getStudentDataDetail.Gender,
                    GenderIndo = getStudentDataDetail.Gender == Gender.Male ? "Laki-laki" : "Perempuan",
                    Religion = getStudentDataDetail.Religion,
                    ReligionIndo = GetReligionIndo(getStudentDataDetail.Religion),
                    ReligionSubject = getStudentDataDetail.ReligionSubject,
                    ReligionSubjectIndo = GetReligionIndo(getStudentDataDetail.ReligionSubject),
                    ChildStatus = GetChildStatus(getStudentDataDetail.ChildStatus),
                    ChildStatusIndo = GetChildStatusIndo(getStudentDataDetail.ChildStatus),
                    ResidenceAddress = getStudentDataDetail.ResidenceAddress,
                    JoinToSchoolDate = getJoinToSchoolDate == null ? "" : (getJoinToSchoolDate?.ToString("dd MMMM yyyy")),
                    JoinToSchoolDateIndo = getJoinToSchoolDate == null ? "" : (getJoinToSchoolDate?.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID"))),
                    JoinToSchoolIdGrade = getEntrySchoolByLevel.EntryIdGrade,
                    JoinToSchoolGrade = getEntrySchoolByLevel.EntryGrade,
                    JoinToSchoolAcademicYear = getEntrySchoolByLevel.EntryAcademicYear,
                    JoinToSchoolIdAcademicYear = getEntrySchoolByLevel.EntryIdAcademicYear,
                    JoinToSchoolIdLevel = getEntrySchoolByLevel.EntryIdLevel,
                    PreviousSchool = getStudentPrevSchool.Name,
                    PreviousSchoolAddress = getStudentPrevSchool.Address,
                },
                StudentEnrollment = new GetStudentInformationForBNSReportResult_StudentEnrollment()
                {
                    School = new GetStudentInformationForBNSReportResult_School()
                    {
                        Id = getStudentDataDetail.SchoolId,
                        Name = getStudentDataDetail.SchoolName,
                        //Description = x.Homeroom.Grade.Level.AcademicYear.School.Name,
                        //Address = x.Homeroom.Grade.Level.AcademicYear.School.Address,
                    },
                    AcademicYear = new CodeWithIdVm()
                    {
                        Id = getStudentDataDetail.AcademicYearId,
                        Code = getStudentDataDetail.AcademicYearCode,
                        Description = getStudentDataDetail.AcademicYearDescription,
                    },
                    Level = new CodeWithIdVm()
                    {
                        Id = getStudentDataDetail.LevelId,
                        Code = getStudentDataDetail.LevelCode,
                        Description = getStudentDataDetail.LevelDescription,
                    },
                    Grade = new CodeWithIdVm()
                    {
                        Id = getStudentDataDetail.GradeId,
                        Code = getStudentDataDetail.GradeCode,
                        Description = getStudentDataDetail.GradeDescription,
                    },
                    Homeroom = new CodeWithIdVm()
                    {
                        Id = getStudentDataDetail.HomeroomId,
                        Code = getStudentDataDetail.HomeroomCode,
                        Description = getStudentDataDetail.HomeroomDescription
                    },

                    Semester = param.Semester
                },
                StudentPhoto = getStudentPhoto
            };

            var retVal = await GetStaffData(getStudentData);

            return retVal;
        }

        private async Task<GetStudentInformationForBNSReportResult_ParentData> GetParentData(GetStudentInformationForBNSReportRequest param)
        {
            var getStudentParentList = await _dbContext.Entity<MsStudentParent>()
                    .Where(x => x.IdStudent == param.IdStudent)
                    .Select(x => new GetStudentInformationForBNSReportResult_ParentDetail
                    {
                        Id = x.Parent.Id,
                        Name = NameUtil.GenerateFullName(x.Parent.FirstName, x.Parent.MiddleName, x.Parent.LastName),
                        IdParentRole = x.Parent.IdParentRole,
                        Occupation = x.Parent.OccupationType.OccupationTypeName,
                        ResidenceAddress = x.Parent.ResidenceAddress,
                        Telephone = x.Parent.MobilePhoneNumber1
                    })
                    .Distinct()
                    .ToListAsync(CancellationToken);

            var getParentData = new GetStudentInformationForBNSReportResult_ParentData()
            {
                Father = getStudentParentList.Where(x => x.IdParentRole == "F").FirstOrDefault(),
                Mother = getStudentParentList.Where(x => x.IdParentRole == "M").FirstOrDefault(),
                Guardian = getStudentParentList.Where(x => x.IdParentRole == "G").FirstOrDefault(),
            };

            return getParentData;
        }

        private async Task<GetStudentInformationForBNSReportResult_Student> GetStaffData(GetStudentInformationForBNSReportResult_Student studentData)
        {
            var getHomeroomTeacher = new NameValueVm();
            var getHomeroomTeacher2 = new NameValueVm();
            var getPrincipalName = new NameValueVm();
            var getCoordinatorName = new NameValueVm();
            var IdPrincipal = string.Empty;
            var IdCoordinator = string.Empty;
            Dictionary<string, string> resultsPrincipal = new Dictionary<string, string>();

            getHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                    .Where(x => x.IdHomeroom == studentData.StudentEnrollment.Homeroom.Id &&
                                x.TeacherPosition.Position.Code == PositionConstant.ClassAdvisor &&
                                x.IsShowInReportCard == true)
                    .Select(x => new NameValueVm()
                    {
                        Id = x.IdBinusian,
                        Name = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                    }).FirstOrDefaultAsync(CancellationToken);

            getHomeroomTeacher2 = await _dbContext.Entity<MsHomeroomTeacher>()
                    .Where(x => x.IdHomeroom == studentData.StudentEnrollment.Homeroom.Id &&
                                (x.TeacherPosition.Position.Code == PositionConstant.CoTeacher || x.TeacherPosition.Position.Code == PositionConstant.ClassAdvisor) &&
                                x.IsShowInReportCard == true &&
                                //x.IdBinusian != getHomeroomTeacher.Id)
                                x.IdBinusian != (getHomeroomTeacher != null ? getHomeroomTeacher.Id : x.IdBinusian))
                    .Select(x => new NameValueVm()
                    {
                        Id = x.IdBinusian,
                        Name = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                    }).FirstOrDefaultAsync(CancellationToken);

            studentData.StudentEnrollment.HomeroomTeacher = getHomeroomTeacher;
            studentData.StudentEnrollment.HomeroomTeacher2 = getHomeroomTeacher2;
            studentData.StudentEnrollment.Principal = await GetBinusianName(studentData, studentData.StudentEnrollment.AcademicYear.Id, studentData.StudentEnrollment.Level.Id, PositionConstant.Principal.ToString());
            studentData.StudentEnrollment.PrincipalFirstYear = await GetBinusianName(studentData, studentData.StudentDetail.JoinToSchoolIdAcademicYear, studentData.StudentDetail.JoinToSchoolIdLevel, PositionConstant.Principal.ToString());
            studentData.StudentEnrollment.Coordinator = await GetBinusianName(studentData, studentData.StudentEnrollment.AcademicYear.Id, studentData.StudentEnrollment.Level.Id, PositionConstant.AffectiveCoordinator.ToString());

            return studentData;
        }

        private string GetReligionIndo(string Religion)
        {
            var getReligionIndo = "";

            switch (Religion)
            {
                case "N/A":
                    getReligionIndo = "N/A";
                    break;
                case "Islam":
                    getReligionIndo = "Islam";
                    break;
                case "Catholic":
                    getReligionIndo = "Katolik";
                    break;
                case "Christian":
                    getReligionIndo = "Kristen Protestan";
                    break;
                case "Hindu":
                    getReligionIndo = "Hindu";
                    break;
                case "Buddha":
                    getReligionIndo = "Buddha";
                    break;
                case "Kong Hu Cu":
                    getReligionIndo = "Khonghucu";
                    break;
                case "Others":
                    getReligionIndo = "Kepercayaan terhadap Tuhan YME";
                    break;
            }

            return getReligionIndo;
        }

        private string GetChildStatus(string IdChildStatus)
        {
            var getChildStatus = "";

            switch (IdChildStatus)
            {
                case "N/A":
                    getChildStatus = "Biological Child";
                    break;
                case "Biological Child":
                    getChildStatus = "Biological Child";
                    break;
                case "Foster Child":
                    getChildStatus = "Foster Child";
                    break;
            }

            return getChildStatus;
        }

        private string GetChildStatusIndo(string ChildStatus)
        {
            var getChildStatusIndo = "";

            switch (ChildStatus)
            {
                case "N/A":
                    getChildStatusIndo = "Anak Kandung";
                    break;
                case "Biological Child":
                    getChildStatusIndo = "Anak Kandung";
                    break;
                case "Foster Child":
                    getChildStatusIndo = "Anak Angkat";
                    break;
            }

            return getChildStatusIndo;
        }

        private string GetIndonesianLevel(string GradeCode)
        {
            var IndonesianLevel = "";

            if (GradeCode.Substring(0, GradeCode.Length - 1) == "EY" || GradeCode.Substring(0, GradeCode.Length - 1) == "ECY")
            {
                if (_pendidikanTK.Contains(GradeCode.Substring(GradeCode.Length - 1)))
                {
                    IndonesianLevel = "TK";
                }
                else
                {
                    IndonesianLevel = "SEKOLAH";
                }
            }
            else
            {
                if (_pendidikanSD.Contains(GradeCode))
                {
                    IndonesianLevel = "SD";
                }
                else if (_pendidikanSMP.Contains(GradeCode))
                {
                    IndonesianLevel = "SMP";
                }
                else if(_pendidikanSMA.Contains(GradeCode))
                {
                    IndonesianLevel = "SMA";
                }
                else
                {
                    IndonesianLevel = "SEKOLAH";
                }
            }

            return IndonesianLevel;
        }

        private async Task<List<GetStudentInformationForBNSReportRequest_EnrollmentStudent>> GetDataStudentAllEnrollment(GetStudentInformationForBNSReportRequest param, string AcademicYearCode)
        {
            var retVal = new List<GetStudentInformationForBNSReportRequest_EnrollmentStudent>();

            var getAllHomeroomStudentList = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                    .Where(x => x.HomeroomStudent.IdStudent == param.IdStudent)
                    .Select(x => new
                    {
                        IdSchool = x.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.MsSchool.Id,
                        SchoolName = x.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.MsSchool.Name,
                        //SchoolDesc = x.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.MsSchool.Description,
                        //SchoolAddress = x.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.MsSchool.Address,
                        IdAcademicYear = x.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Id,
                        AcademicYearCode = x.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Code,
                        AcademicYearDesc = x.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                        IdLevel = x.HomeroomStudent.Homeroom.Grade.MsLevel.Id,
                        LevelCode = x.HomeroomStudent.Homeroom.Grade.MsLevel.Code,
                        LevelDesc = x.HomeroomStudent.Homeroom.Grade.MsLevel.Description,
                        IdGrade = x.HomeroomStudent.Homeroom.Grade.Id,
                        GradeCode = x.HomeroomStudent.Homeroom.Grade.Code,
                        GradeDesc = x.HomeroomStudent.Homeroom.Grade.Description,
                        Semester = x.HomeroomStudent.Semester
                    }).Distinct()
                    .OrderByDescending(x => x.AcademicYearCode)
                    .ThenByDescending(x => x.Semester)
                    .ToListAsync(CancellationToken);

            foreach (var item in getAllHomeroomStudentList)
            {
                if (int.Parse(item.AcademicYearCode) <= int.Parse(AcademicYearCode))
                {
                    var IndonesianLevel = GetIndonesianLevel(item.GradeCode);

                    var dataHomeroomStudent = new GetStudentInformationForBNSReportRequest_EnrollmentStudent()
                    {
                        IdSchool = item.IdSchool,
                        SchoolName = item.SchoolName,
                        IdAcademicYear = item.IdAcademicYear,
                        AcademicYearCode = item.AcademicYearCode,
                        AcademicYearDesc = item.AcademicYearDesc,
                        IndonesianLevel = IndonesianLevel,
                        IdLevel = item.IdLevel,
                        LevelCode = item.LevelCode,
                        LevelDesc = item.LevelDesc,
                        IdGrade = item.IdGrade,
                        GradeCode = item.GradeCode,
                        GradeDesc = item.GradeDesc,
                        Semester = item.Semester
                    };

                    retVal.Add(dataHomeroomStudent);
                }
            }

            return retVal;
        }

        private async Task<DateTime?> GetJoinToSchoolDate(GetStudentInformationForBNSReportRequest param, List<GetStudentInformationForBNSReportRequest_EnrollmentStudent> EnrollmentStudent)
        {
            var getActiveCodeLevel = EnrollmentStudent.Where(x => x.IdGrade == param.IdGrade).OrderByDescending(x => x.Semester).FirstOrDefault();

            var getFirstGrade = EnrollmentStudent
                    .Where(x => x.IndonesianLevel.Contains(getActiveCodeLevel.IndonesianLevel))
                    .OrderByDescending(x => x.AcademicYearCode)
                    .Last();

            var getAdmissionData = await _dbContext.Entity<MsAdmissionData>()
                    .Where(x => x.IdStudent == param.IdStudent)
                    .Select(x => x.JoinToSchoolDate)
                    .FirstOrDefaultAsync(CancellationToken);

            var getPeriodData = await _dbContext.Entity<MsPeriod>()
                    .Where(x => x.IdGrade == getFirstGrade.IdGrade &&
                                x.Semester == getFirstGrade.Semester)
                    .OrderBy(x => x.Code)
                    .Select(x => new
                    {
                        StartDate = x.StartDate,
                        Term = x.Code,
                        Semester = x.Semester
                    }).Distinct()
                    .ToListAsync(CancellationToken);

            var getJoinToSchoolDate = getPeriodData.FirstOrDefault();

            var JoinSchoolDate = new DateTime?();

            if (getAdmissionData != null)
            {
                JoinSchoolDate = getJoinToSchoolDate.StartDate > getAdmissionData ? getJoinToSchoolDate.StartDate : getAdmissionData;
            }
            else
            {
                JoinSchoolDate = getJoinToSchoolDate.StartDate;
            }

            return JoinSchoolDate;
        }

        private GetStudentInformationForBNSReportResult_EntryGradeInSameLevel GetEntrySchoolByLevel(GetStudentInformationForBNSReportRequest param, List<GetStudentInformationForBNSReportRequest_EnrollmentStudent> EnrollmentStudent)
        {
            var retVal = new GetStudentInformationForBNSReportResult_EntryGradeInSameLevel();
            var getJoinToSchoolGrade = "";
            var getActiveCodeLevel = EnrollmentStudent.Where(x => x.IdGrade == param.IdGrade).FirstOrDefault();

            var getLevelList = EnrollmentStudent
                    .Select(x => new {
                        IdAcademicYear = x.IdAcademicYear,
                        AcademicYearCode = x.AcademicYearCode,
                        AcademicYearDesc = x.AcademicYearDesc,
                        IndonesianLevel = x.IndonesianLevel,
                        IdLevel = x.IdLevel,
                        LevelCode = x.LevelCode,
                        IdGrade = x.IdGrade,
                        GradeCode = x.GradeCode,
                    }).Distinct()
                    .OrderBy(x => x.AcademicYearCode)
                    .ToList();

            if (getActiveCodeLevel.IndonesianLevel == "TK")
            {
                var getJoinToSchool = getLevelList
                        .Where(x => x.IndonesianLevel.Contains("TK"))
                        .Select(x => new { x.IdAcademicYear, x.AcademicYearDesc, x.IdLevel, x.LevelCode, x.IdGrade, x.GradeCode})
                        .FirstOrDefault();

                if (getActiveCodeLevel.LevelCode == "ECY" || getActiveCodeLevel.LevelCode.Substring(0, getActiveCodeLevel.LevelCode.Length - 1) == "ECY")
                {
                    getJoinToSchoolGrade = "ECY " + getJoinToSchool.GradeCode.Substring(getJoinToSchool.GradeCode.Length - 1);
                }
                else if (getActiveCodeLevel.LevelCode == "EY")
                {
                    getJoinToSchoolGrade = "EY " + getJoinToSchool.GradeCode.Substring(getJoinToSchool.GradeCode.Length - 1);
                }

                retVal.EntryIdAcademicYear = getJoinToSchool.IdAcademicYear;
                retVal.EntryAcademicYear = getJoinToSchool.AcademicYearDesc;
                retVal.EntryIdLevel = getJoinToSchool.IdLevel;
                retVal.EntryLevel = getJoinToSchool.LevelCode;
                retVal.EntryIdGrade = getJoinToSchool.IdGrade;
                retVal.EntryGrade = getJoinToSchoolGrade;
            }
            else if (getActiveCodeLevel.IndonesianLevel == "SD")
            {
                var getJoinToSchool = getLevelList
                        .Where(x => x.IndonesianLevel.Contains("SD"))
                        .Select(x => new { x.IdAcademicYear, x.AcademicYearDesc, x.IdLevel, x.LevelCode, x.IdGrade, x.GradeCode })
                        .FirstOrDefault();

                getJoinToSchoolGrade = getJoinToSchool.GradeCode.Replace(getActiveCodeLevel.LevelCode, "");
                retVal.EntryIdAcademicYear = getJoinToSchool.IdAcademicYear;
                retVal.EntryAcademicYear = getJoinToSchool.AcademicYearDesc;
                retVal.EntryIdLevel = getJoinToSchool.IdLevel;
                retVal.EntryLevel = getJoinToSchool.LevelCode;
                retVal.EntryIdGrade = getJoinToSchool.IdGrade;
                retVal.EntryGrade = getJoinToSchoolGrade;
            }
            else if (getActiveCodeLevel.IndonesianLevel == "SMP")
            {
                var getJoinToSchool = getLevelList
                        .Where(x => x.IndonesianLevel.Contains("SMP"))
                        .Select(x => new { x.IdAcademicYear, x.AcademicYearDesc, x.IdLevel, x.LevelCode, x.IdGrade, x.GradeCode })
                        .FirstOrDefault();

                getJoinToSchoolGrade = getJoinToSchool.GradeCode.Replace(getActiveCodeLevel.LevelCode, "");
                retVal.EntryIdAcademicYear = getJoinToSchool.IdAcademicYear;
                retVal.EntryAcademicYear = getJoinToSchool.AcademicYearDesc;
                retVal.EntryIdLevel = getJoinToSchool.IdLevel;
                retVal.EntryLevel = getJoinToSchool.LevelCode;
                retVal.EntryIdGrade = getJoinToSchool.IdGrade;
                retVal.EntryGrade = getJoinToSchoolGrade;
            }
            else if (getActiveCodeLevel.IndonesianLevel == "SMA")
            {
                var getJoinToSchool = getLevelList
                        .Where(x => x.IndonesianLevel.Contains("SMA"))
                        .Select(x => new { x.IdAcademicYear, x.AcademicYearDesc, x.IdLevel, x.LevelCode, x.IdGrade, x.GradeCode })
                        .FirstOrDefault();

                getJoinToSchoolGrade = getJoinToSchool.GradeCode.Replace(getActiveCodeLevel.LevelCode, "");
                retVal.EntryIdAcademicYear = getJoinToSchool.IdAcademicYear;
                retVal.EntryAcademicYear = getJoinToSchool.AcademicYearDesc;
                retVal.EntryIdLevel = getJoinToSchool.IdLevel;
                retVal.EntryLevel = getJoinToSchool.LevelCode;
                retVal.EntryIdGrade = getJoinToSchool.IdGrade;
                retVal.EntryGrade = getJoinToSchoolGrade;
            }
            else
            {
                var getJoinToSchool = getLevelList
                        .Where(x => x.IndonesianLevel == getActiveCodeLevel.IndonesianLevel)
                        .Select(x => new { x.IdAcademicYear, x.AcademicYearDesc, x.IdLevel, x.LevelCode, x.IdGrade, x.GradeCode })
                        .FirstOrDefault();

                getJoinToSchoolGrade = getJoinToSchool.GradeCode.Replace(getActiveCodeLevel.LevelCode, "");
                retVal.EntryIdAcademicYear = getJoinToSchool.IdAcademicYear;
                retVal.EntryAcademicYear = getJoinToSchool.AcademicYearDesc;
                retVal.EntryIdLevel = getJoinToSchool.IdLevel;
                retVal.EntryLevel = getJoinToSchool.LevelCode;
                retVal.EntryIdGrade = getJoinToSchool.IdGrade;
                retVal.EntryGrade = getJoinToSchoolGrade;
            }

            return retVal;
        }

        private async Task<GetStudentInformationForBNSReportResult_School> GetStudentPrevSchool(GetStudentInformationForBNSReportRequest param, List<GetStudentInformationForBNSReportRequest_EnrollmentStudent> EnrollmentStudent)
        {
            var getPrevSchool = new GetStudentInformationForBNSReportResult_School();
            var getPrevCodeLevel = new GetStudentInformationForBNSReportRequest_EnrollmentStudent();

            var getRequestedCodeLevel = EnrollmentStudent.Where(x => x.IdGrade == param.IdGrade).FirstOrDefault();
            var IsRequestedCodeLevelTK = getRequestedCodeLevel.IndonesianLevel.Trim() == "TK" ? true : false;

            if (IsRequestedCodeLevelTK)
            {
                getPrevCodeLevel = EnrollmentStudent
                        .Where(x => int.Parse(x.AcademicYearCode) < int.Parse(getRequestedCodeLevel.AcademicYearCode))
                        .OrderByDescending(x => x.AcademicYearCode)
                        .FirstOrDefault();
            }
            else
            {
                getPrevCodeLevel = EnrollmentStudent
                        .Where(x => x.IndonesianLevel != getRequestedCodeLevel.IndonesianLevel)
                        .OrderByDescending(x => x.AcademicYearCode)
                        .FirstOrDefault();
            }

            if (getPrevCodeLevel != null)
            {
                getPrevSchool.Id = getPrevCodeLevel.IdSchool;
                getPrevSchool.Description = getPrevCodeLevel.SchoolDesc;
                getPrevSchool.Address = "Jl. Lengkong Karya - Jelupang No.58"; //Hardcode Alamat Serpong

                if (getPrevCodeLevel.IndonesianLevel.Trim() == "TK")
                {
                    if (IsRequestedCodeLevelTK == true)
                    {
                        getPrevSchool.Id = "-";
                        getPrevSchool.Description = "-";
                        getPrevSchool.Address = "-";
                        getPrevSchool.Name = "-";
                    }
                    else
                    {
                        getPrevSchool.Name = "TK BINA NUSANTARA " + getPrevCodeLevel.SchoolName;
                    }
                    
                }
                else if (getPrevCodeLevel.IndonesianLevel.Trim() == "SD")
                {
                    getPrevSchool.Name = "SD BINA NUSANTARA " + getPrevCodeLevel.SchoolName;
                }
                else if (getPrevCodeLevel.IndonesianLevel.Trim() == "SMP")
                {
                    getPrevSchool.Name = "SMP BINA NUSANTARA " + getPrevCodeLevel.SchoolName;
                }
                else
                {
                    getPrevSchool.Name = "SEKOLAH BINA NUSANTARA";
                }
            }
            else
            {
                var getStudentPrevSchool = await _dbContext.Entity<MsStudentPrevSchoolInfo>()
                        .Where(x => x.IdStudent == param.IdStudent)
                        .Select(x => new
                        {
                            Grade = x.Grade,
                            YearAttended = x.YearAttended,
                            YearWithdrawn = x.YearWithdrawn,
                            IsHomeSchooling = x.IsHomeSchooling,
                            IdPreviousSchoolNew = x.IdPreviousSchoolNew,
                            IdPreviousSchoolOld = x.IdPreviousSchoolOld
                        }).Distinct()
                        .FirstOrDefaultAsync(CancellationToken);

                var getPrevSchoolName = await _dbContext.Entity<MsPreviousSchoolNew>()
                        .Where(x => x.Id == (getStudentPrevSchool == null ? null : getStudentPrevSchool.IdPreviousSchoolNew))
                        .Distinct()
                        .FirstOrDefaultAsync(CancellationToken);

                var getPrevSchoolNameOld = await _dbContext.Entity<MsPreviousSchoolOld>()
                        .Where(x => x.Id == (getStudentPrevSchool == null ? null : getStudentPrevSchool.IdPreviousSchoolOld))
                        .Distinct()
                        .FirstOrDefaultAsync(CancellationToken);

                getPrevSchool.Id = getPrevSchoolName?.Id ?? (getPrevSchoolNameOld?.Id ?? "-");
                getPrevSchool.Name = getPrevSchoolName?.SchoolName ?? (getPrevSchoolNameOld?.SchoolName ?? "-");
                getPrevSchool.Description = getPrevSchoolName?.SchoolName ?? getPrevSchoolNameOld?.SchoolName ?? "-";
                getPrevSchool.Address = getPrevSchoolName?.Address ?? "-";
            }

            return getPrevSchool;
        }

        private async Task<NameValueVm> GetBinusianName(GetStudentInformationForBNSReportResult_Student studentData, string IdAcademicYear, string IdLevel, string Position)
        {
            var getTrNonTeachingLoadList = await _dbContext.Entity<TrNonTeachingLoad>()
                    .Where(x => x.MsNonTeachingLoad.AcademicYear.IdSchool == studentData.StudentEnrollment.School.Id)
                    .Select(x => new
                    {
                        IdAcademicYear = x.MsNonTeachingLoad.IdAcademicYear,
                        Id = x.IdUser,
                        Code = x.MsNonTeachingLoad.TeacherPosition.Position.Code,
                        Data = x.Data
                    }).Distinct()
                    .ToListAsync(CancellationToken);

            var getTrNonTeachingLoad = getTrNonTeachingLoadList
                    .Where(x => x.IdAcademicYear == IdAcademicYear)
                    .Select(x => new
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Data = x.Data
                    }).Distinct().ToList();

            var getBinusianData = getTrNonTeachingLoad
                    .Where(x => x.Code == Position)
                    .Select(x => new
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Data = x.Data
                    }).Distinct().ToList();

            var getBinusianName = new NameValueVm();
            var IdBinusian = string.Empty;

            Dictionary<string, string> resultsBinusian = new Dictionary<string, string>();

            foreach (var dataPrincipal in getBinusianData)
            {
                var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(dataPrincipal.Data);
                _dataNewLH.TryGetValue("Level", out var _levelLH);

                if (!resultsBinusian.ContainsKey(_levelLH.Id))
                {
                    resultsBinusian.Add(_levelLH.Id, dataPrincipal.Id);
                }
            }

            if (resultsBinusian.TryGetValue(IdLevel, out IdBinusian))
            {
                getBinusianName = await _dbContext.Entity<MsStaff>()
                        .Where(x => x.IdBinusian == IdBinusian)
                        .Select(x => new NameValueVm()
                        {
                            Id = x.IdBinusian,
                            Name = NameUtil.GenerateFullName(x.FirstName, x.LastName)
                        }).FirstOrDefaultAsync(CancellationToken);
            }

            return getBinusianName;
        }

        private List<string> GetAllAYByLevelIndo(List<GetStudentInformationForBNSReportRequest_EnrollmentStudent> getDataStudentAllEnrollment, string IdGrade)
        {
            var retVal = new List<string>();
            var getActiveCodeLevel = getDataStudentAllEnrollment.Where(x => x.IdGrade == IdGrade).FirstOrDefault();

            var getLevelList = getDataStudentAllEnrollment
                    .Select(x => new {
                        IdAcademicYear = x.IdAcademicYear,
                        AcademicYearCode = x.AcademicYearCode,
                        AcademicYearDesc = x.AcademicYearDesc,
                        IndonesianLevel = x.IndonesianLevel,
                        IdLevel = x.IdLevel,
                        LevelCode = x.LevelCode,
                        IdGrade = x.IdGrade,
                        GradeCode = x.GradeCode,
                    }).Distinct()
                    .OrderBy(x => x.AcademicYearCode)
                    .ToList();

            if (getActiveCodeLevel.IndonesianLevel == "TK")
            {
                retVal = getLevelList
                        .Where(x => x.IndonesianLevel.Contains("TK"))
                        .Select(x => x.IdAcademicYear)
                        .OrderByDescending(x => x)
                        .Distinct()
                        .ToList();
            }
            else if (getActiveCodeLevel.IndonesianLevel == "SD")
            {
                retVal = getLevelList
                        .Where(x => x.IndonesianLevel.Contains("SD"))
                        .Select(x => x.IdAcademicYear)
                        .OrderByDescending(x => x)
                        .Distinct()
                        .ToList();
            }
            else if (getActiveCodeLevel.IndonesianLevel == "SMP")
            {
                retVal = getLevelList
                        .Where(x => x.IndonesianLevel.Contains("SMP"))
                        .Select(x => x.IdAcademicYear)
                        .OrderByDescending(x => x)
                        .Distinct()
                        .ToList();
            }
            else if (getActiveCodeLevel.IndonesianLevel == "SMA")
            {
                retVal = getLevelList
                        .Where(x => x.IndonesianLevel.Contains("SMA"))
                        .Select(x => x.IdAcademicYear)
                        .OrderByDescending(x => x)
                        .Distinct()
                        .ToList();
            }
            else
            {
                retVal = getLevelList
                        .Select(x => x.IdAcademicYear)
                        .OrderByDescending(x => x)
                        .Distinct()
                        .ToList();
            }

            return retVal;
        }

        private async Task<GetStudentInformationForBNSReportResult_StudentPhoto> GetStudentPhoto(string AcademicYearCode, string IdStudent)
        {
            //var getAllAYByLevelIndo = GetAllAYByLevelIndo(getDataStudentAllEnrollment, param.IdGrade);

            var IntAcademicYearCode = int.Parse(AcademicYearCode);

            var getStudentPhoto = await _dbContext.Entity<TrStudentPhoto>()
                    .Where(x => x.IdStudent == IdStudent)
                    .Select(x => new
                    {
                        IdStudent = x.IdStudent,
                        IdAcademicYear = x.IdAcademicYear,
                        AcademicYearCode = x.AcademicYear.Code,
                        FileName = x.FileName,
                        FilePath = x.FilePath,
                    }).Distinct()
                    .OrderByDescending (x => x.IdAcademicYear)
                    .ToListAsync(CancellationToken);

            var result = getStudentPhoto
                    .Where(x => Convert.ToInt32(x.AcademicYearCode) <= IntAcademicYearCode)
                    .OrderByDescending(x => x.IdAcademicYear)
                    .FirstOrDefault();

            var retVal = result == null ? null : new GetStudentInformationForBNSReportResult_StudentPhoto()
            {
                IdAcademicYear = result.IdAcademicYear,
                FileName = result.FileName,
                FilePath = result.FilePath
            };

            return retVal;
        }
    }
}
