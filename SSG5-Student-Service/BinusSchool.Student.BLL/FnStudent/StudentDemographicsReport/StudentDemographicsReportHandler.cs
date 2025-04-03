using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.StudentDemographicsReport.Validator;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport
{
    public class StudentDemographicsReportHandler : FunctionsHttpSingleHandler
    {
        private const string _IdTypeInternalIntake = "1";
        private const string _IdTypeExternalIntake = "2";
        private const string _IdTypeInactive = "3";
        private const string _IdTypeWithdrawalProcess = "4";
        private const string _IdTypeTransfer = "5";
        private const string _IdTypeActive = "6";
        private const string _IdTypeWithdrawn = "7";
        private const string _IdTypeTotalStudents = "8";

        private readonly IStudentDbContext _dbContext;
        public StudentDemographicsReportHandler(
            IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<StudentDemographicsReportRequest, StudentDemographicsReportValidator>();

            var result = await StudentDemographicsReport(new StudentDemographicsReportRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester,
                Level = param.Level,
                Grade = param.Grade,
                ViewCategoryType = param.ViewCategoryType,
                TotalStudent = param.TotalStudent,
                TotalStudentDetail = param.TotalStudentDetail
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<StudentDemographicsReportResult> StudentDemographicsReport(StudentDemographicsReportRequest param)
        {
            var retVal = new StudentDemographicsReportResult();

            if (string.IsNullOrWhiteSpace(param.ViewCategoryType))
                throw new BadRequestException("View Category Type cannot be empty");

            if (param.ViewCategoryType.Trim().ToLower() != "grade" && param.ViewCategoryType.Trim().ToLower() != "homeroom")
                throw new BadRequestException("Wrong View Category Type");

            var getStudentTotalReport = new GetSDRTotalStudentReportsResult();
            var getStudentTotalReportSemestersDataList = new List<GetSDRTotalStudentReportsResult_SemestersData>();

            if (param.TotalStudent)
            {
                getStudentTotalReport.ViewCategoryType = param.ViewCategoryType;

                int s = 0;
                int n = 0;

                if (param.Semester == null)
                {
                    s = 1;
                    n = 2;
                }
                else
                {
                    s = (int)param.Semester;
                    n = (int)param.Semester;
                }

                for (int smt = s; smt <= n; smt++)
                {
                    var semestersData = param.ViewCategoryType.Trim().ToLower() == "grade" ?
                        await SetSDRTotalStudentByGrade(smt, param) :
                        await SetSDRTotalStudentByHomeroom(smt, param);

                    getStudentTotalReportSemestersDataList.Add(semestersData);
                }

                getStudentTotalReport.SemestersData = getStudentTotalReportSemestersDataList;
                retVal.SDRTotalStudentReports = getStudentTotalReport;
            }
            else if (param.TotalStudentDetail)
            {
                if (param.Semester == null)
                    throw new BadRequestException("Semester cannot be empty");

                //if ((param.ViewCategoryType.Trim().ToLower() == "grade" && param.Grade == null) ||
                //    (param.ViewCategoryType.Trim().ToLower() == "homeroom" && param.Homeroom == null))
                //    throw new BadRequestException("Grade or Homeroom cannot be empty");

                var details = await SetSDRTotalStudentDetails((int)param.Semester, param);
                retVal.SDRTotalStudentReportDetails = details;
            }

            return retVal;
        }

        public async Task<List<StudentDemographicsReportResult_Homeroom>> SetHomeroomData(StudentDemographicsReportRequest param, int semester)
        {
            var dataResult = new List<StudentDemographicsReportResult_Homeroom>();

            var getPeriod = await _dbContext.Entity<MsPeriod>()
                .Where(a => a.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                            && (param.Level == null || param.Level.Contains(a.Grade.IdLevel))
                            && (param.Grade == null || param.Grade.Contains(a.IdGrade)))
                .OrderBy(a => a.Semester)
                .ThenBy(a => a.Code)
                .ToListAsync();

            var getHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(a => a.Student)
                .Include(a => a.Homeroom.Grade.MsLevel)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                            && (param.Level == null || param.Level.Contains(a.Homeroom.Grade.IdLevel))
                            && (param.Grade == null || param.Grade.Contains(a.Homeroom.IdGrade))
                            && (param.Homeroom == null || param.Homeroom.Contains(a.Homeroom.Id))
                            && a.Semester == semester)
                .ToListAsync();

            var joinData = from homeroomStudent in getHomeroomStudent
                           join period in getPeriod
                           on new { homeroomStudent.Homeroom.IdGrade, homeroomStudent.Semester } equals new { period.IdGrade, period.Semester } into periodGroup
                           from period in periodGroup.DefaultIfEmpty()
                           select new
                           {
                               Student = homeroomStudent,
                               Period = period
                           };

            var data = joinData.Select(a => new StudentDemographicsReportResult_Homeroom
            {
                IdLevel = a.Student.Homeroom.Grade.IdLevel,
                LevelName = a.Student.Homeroom.Grade.MsLevel.Description,
                IdGrade = a.Student.Homeroom.IdGrade,
                GradeName = a.Student.Homeroom.Grade.Description,
                GradeCode = a.Student.Homeroom.Grade.Code,
                IdHomeroom = a.Student.IdHomeroom,
                HomeroomName = a.Student.Homeroom.Grade.Code + a.Student.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                Semester = a.Student.Semester,
                Term = a.Period?.Code,
                StartDate = a.Period?.StartDate,
                EndDate = a.Period?.EndDate
            });

            dataResult.AddRange(data);

            return dataResult;
        }

        public async Task<List<StudentDemographicsReportResult_HomeroomStudentAdmissionData>> SetDataHomeroomAdmission(StudentDemographicsReportRequest param, int paramSmt)
        {
            var dataResult = new List<StudentDemographicsReportResult_HomeroomStudentAdmissionData>();

            var getPeriod = await _dbContext.Entity<MsPeriod>()
                .Where(a => a.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                            && (param.Level == null || param.Level.Contains(a.Grade.IdLevel))
                            && (param.Grade == null || param.Grade.Contains(a.IdGrade)))
                .OrderBy(a => a.Semester)
                .ThenBy(a => a.Code)
                .ToListAsync();

            var firstDaySchool = getPeriod.OrderBy(a => a.StartDate).Select(a => a.StartDate).FirstOrDefault();
            var lastDaySchool = getPeriod.OrderByDescending(a => a.EndDate).Select(a => a.EndDate).FirstOrDefault();

            var getIdSchool = await _dbContext.Entity<MsAcademicYear>()
                .Where(a => a.Id == param.IdAcademicYear)
                .Select(a => a.IdSchool)
                .FirstOrDefaultAsync();

            var getAdmissionData = await _dbContext.Entity<MsAdmissionData>()
                .Where(x => x.IdSchool == getIdSchool &&
                            (paramSmt == 1 ? x.JoinToSchoolDate >= firstDaySchool : x.JoinToSchoolDate >= firstDaySchool && x.JoinToSchoolDate <= lastDaySchool) &&
                            (paramSmt.ToString() == null || paramSmt == 1 || x.IdAcademicSemester == x.IdAcademicSemester))
                .ToListAsync();

            var getHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(a => a.Student)
                .Include(a => a.Homeroom.Grade.MsLevel)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                            && (param.Level == null || param.Level.Contains(a.Homeroom.Grade.IdLevel))
                            && (param.Grade == null || param.Grade.Contains(a.Homeroom.IdGrade))
                            && (param.Homeroom == null || param.Homeroom.Contains(a.Homeroom.Id))
                            && a.Semester == paramSmt)
                .ToListAsync();

            var joinData = from homeroomStudent in getHomeroomStudent
                           join admission in getAdmissionData
                           on homeroomStudent.IdStudent equals admission.IdStudent into admissionGroup
                           from admission in admissionGroup.DefaultIfEmpty()
                           select new
                           {
                               Student = homeroomStudent,
                               Admission = admission
                           };

            var data = joinData.Select(a => new StudentDemographicsReportResult_HomeroomStudentAdmissionData
            {
                IdLevel = a.Student.Homeroom.Grade.IdLevel,
                LevelName = a.Student.Homeroom.Grade.MsLevel.Description,
                IdGrade = a.Student.Homeroom.IdGrade,
                GradeName = a.Student.Homeroom.Grade.Description,
                GradeCode = a.Student.Homeroom.Grade.Code,
                IdHomeroom = a.Student.IdHomeroom,
                HomeroomName = a.Student.Homeroom.Grade.Code + a.Student.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                IdStudent = a.Student.IdStudent,
                StudentName = NameUtil.GenerateFullName(a.Student.Student.FirstName, a.Student.Student.LastName),
                IdStudentStatus = a.Student.Student.IdStudentStatus,
                IdStudentAdmission = a.Admission?.IdStudent,
                JoinToSchoolDate = a.Admission?.JoinToSchoolDate,
                Semester = a.Student.Semester
            });

            dataResult.AddRange(data);

            return dataResult;
        }

        public async Task<GetSDRTotalStudentReportsResult_SemestersData> SetSDRTotalStudentByGrade(int paramSmt, StudentDemographicsReportRequest param)
        {
            var getStudentTotalReportSemestersData = new GetSDRTotalStudentReportsResult_SemestersData();

            var getStudentTotalReportListDate = new List<GetSDRTotalStudentReportsResult_ListData>();
            var getStudentTotalReportRowTotal = new GetSDRTotalStudentReportsResult_RowTotal()
            {
                InternalIntake = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeInternalIntake },
                ExternalIntake = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeExternalIntake },
                Inactive = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeInactive },
                WithdrawalProcess = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeWithdrawalProcess },
                Transfer = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeTransfer },
                Active = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeActive },
                Withdrawn = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeWithdrawn },
                TotalStudents = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeTotalStudents },
            };

            var GetDataHomeroomFromStudentDemographicsReport = await SetHomeroomData(param, paramSmt);
            var GetDataHomeroomFromStudentDemographicsReportDetail = await SetDataHomeroomAdmission(param, paramSmt);

            var getAccessByGrade = GetDataHomeroomFromStudentDemographicsReport
                    .Select(x => new
                    {
                        IdGrade = x.IdGrade,
                        GradeName = x.GradeName,
                        GradeCode = x.GradeCode
                    }).OrderBy(x => x.GradeName.Length)
                        .ThenBy(x => x.GradeName)
                    .Distinct().ToList();

            foreach (var item in getAccessByGrade)
            {
                var getPeriodStartDate = GetDataHomeroomFromStudentDemographicsReport.Where(x => x.IdGrade == item.IdGrade).OrderBy(x => x.Semester).ThenBy(x => x.StartDate).Select(x => x.StartDate).FirstOrDefault();

                var getHomeroomStudentDataPerGrade = GetDataHomeroomFromStudentDemographicsReportDetail
                        .Where(x => x.IdGrade == item.IdGrade && x.Semester == paramSmt)
                        .ToList();

                var getStudentInternalIntake = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeInternalIntake, getPeriodStartDate);
                var getStudentExternalIntake = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeExternalIntake, getPeriodStartDate);
                var getStudentInactive = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeInactive, getPeriodStartDate);
                var getStudentWithdrawalProcess = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeWithdrawalProcess, getPeriodStartDate);
                var getStudentTransfer = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeTransfer, getPeriodStartDate);
                var getStudentActive = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeActive, getPeriodStartDate);
                var getStudentWithdrawn = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeWithdrawn, getPeriodStartDate);
                var getStudentTotalStudents = getStudentActive.Count() + getStudentWithdrawn.Count();
                var getStudentTotalReportDate = new GetSDRTotalStudentReportsResult_ListData();

                getStudentTotalReportDate.CategoryType = new ItemValueVm { Id = item.IdGrade, Description = item.GradeName };
                getStudentTotalReportDate.InternalIntake = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeInternalIntake, Value = getStudentInternalIntake.Count() };
                getStudentTotalReportDate.ExternalIntake = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeExternalIntake, Value = getStudentExternalIntake.Count() };
                getStudentTotalReportDate.Inactive = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeInactive, Value = getStudentInactive.Count() };
                getStudentTotalReportDate.WithdrawalProcess = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeWithdrawalProcess, Value = getStudentWithdrawalProcess.Count() };
                getStudentTotalReportDate.Transfer = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeTransfer, Value = getStudentTransfer.Count() };
                getStudentTotalReportDate.Active = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeActive, Value = getStudentActive.Count() };
                getStudentTotalReportDate.Withdrawn = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeWithdrawn, Value = getStudentWithdrawn.Count() };
                getStudentTotalReportDate.TotalStudents = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeTotalStudents, Value = getStudentTotalStudents };

                getStudentTotalReportRowTotal.InternalIntake.Value += getStudentInternalIntake.Count();
                getStudentTotalReportRowTotal.ExternalIntake.Value += getStudentExternalIntake.Count();
                getStudentTotalReportRowTotal.Inactive.Value += getStudentInactive.Count();
                getStudentTotalReportRowTotal.WithdrawalProcess.Value += getStudentWithdrawalProcess.Count();
                getStudentTotalReportRowTotal.Transfer.Value += getStudentTransfer.Count();
                getStudentTotalReportRowTotal.Active.Value += getStudentActive.Count();
                getStudentTotalReportRowTotal.Withdrawn.Value += getStudentWithdrawn.Count();
                getStudentTotalReportRowTotal.TotalStudents.Value += getStudentTotalStudents;

                getStudentTotalReportListDate.Add(getStudentTotalReportDate);
            }

            getStudentTotalReportSemestersData.Semester = paramSmt.ToString();
            getStudentTotalReportSemestersData.ListData = getStudentTotalReportListDate;
            getStudentTotalReportSemestersData.RowTotal = getStudentTotalReportRowTotal;

            return getStudentTotalReportSemestersData;
        }

        public async Task<GetSDRTotalStudentReportsResult_SemestersData> SetSDRTotalStudentByHomeroom(int paramSmt, StudentDemographicsReportRequest param)
        {
            var getStudentTotalReportSemestersData = new GetSDRTotalStudentReportsResult_SemestersData();

            var getStudentTotalReportListDate = new List<GetSDRTotalStudentReportsResult_ListData>();
            var getStudentTotalReportRowTotal = new GetSDRTotalStudentReportsResult_RowTotal()
            {
                InternalIntake = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeInternalIntake },
                ExternalIntake = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeExternalIntake },
                Inactive = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeInactive },
                WithdrawalProcess = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeWithdrawalProcess },
                Transfer = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeTransfer },
                Active = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeActive },
                Withdrawn = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeWithdrawn },
                TotalStudents = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeTotalStudents },
            };

            var GetDataHomeroomFromStudentDemographicsReport = await SetHomeroomData(param, paramSmt);
            var GetDataHomeroomFromStudentDemographicsReportDetail = await SetDataHomeroomAdmission(param, paramSmt);

            var getAccessByHomeroom = GetDataHomeroomFromStudentDemographicsReport
                    .Select(x => new
                    {
                        IdGrade = x.IdGrade,
                        GradeName = x.GradeName,
                        GradeCode = x.GradeCode,
                        IdHomeroom = x.IdHomeroom,
                        HomeroomName = x.HomeroomName
                    })
                        .OrderBy(a => a.GradeName.Length)
                        .ThenBy(a => a.GradeName)
                        .ThenBy(x => x.HomeroomName.Length)
                        .ThenBy(x => x.HomeroomName)
                    .Distinct().ToList();

            foreach (var item in getAccessByHomeroom)
            {
                var getPeriodStartDate = GetDataHomeroomFromStudentDemographicsReport.Where(x => x.IdGrade == item.IdGrade).OrderBy(x => x.Semester).ThenBy(x => x.StartDate).Select(x => x.StartDate).FirstOrDefault();

                var getHomeroomStudentDataPerGrade = GetDataHomeroomFromStudentDemographicsReportDetail
                        .Where(x => x.IdGrade == item.IdGrade && x.IdHomeroom == item.IdHomeroom && x.Semester == paramSmt)
                        .ToList();

                var getStudentInternalIntake = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeInternalIntake, getPeriodStartDate);
                var getStudentExternalIntake = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeExternalIntake, getPeriodStartDate);
                var getStudentInactive = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeInactive, getPeriodStartDate);
                var getStudentWithdrawalProcess = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeWithdrawalProcess, getPeriodStartDate);
                var getStudentTransfer = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeTransfer, getPeriodStartDate);
                var getStudentActive = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeActive, getPeriodStartDate);
                var getStudentWithdrawn = await SetHomeroomStudentAdmissionWithStatusType(getHomeroomStudentDataPerGrade, _IdTypeWithdrawn, getPeriodStartDate);
                var getStudentTotalStudents = getStudentActive.Count() + getStudentWithdrawn.Count();
                var getStudentTotalReportDate = new GetSDRTotalStudentReportsResult_ListData();

                getStudentTotalReportDate.CategoryType = new ItemValueVm { Id = item.IdHomeroom, Description = item.HomeroomName };
                getStudentTotalReportDate.InternalIntake = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeInternalIntake, Value = getStudentInternalIntake.Count() };
                getStudentTotalReportDate.ExternalIntake = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeExternalIntake, Value = getStudentExternalIntake.Count() };
                getStudentTotalReportDate.Inactive = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeInactive, Value = getStudentInactive.Count() };
                getStudentTotalReportDate.WithdrawalProcess = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeWithdrawalProcess, Value = getStudentWithdrawalProcess.Count() };
                getStudentTotalReportDate.Transfer = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeTransfer, Value = getStudentTransfer.Count() };
                getStudentTotalReportDate.Active = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeActive, Value = getStudentActive.Count() };
                getStudentTotalReportDate.Withdrawn = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeWithdrawn, Value = getStudentWithdrawn.Count() };
                getStudentTotalReportDate.TotalStudents = new GetSDRTotalStudentReportsResult_IdValue { IdType = _IdTypeTotalStudents, Value = getStudentTotalStudents };

                getStudentTotalReportRowTotal.InternalIntake.Value += getStudentInternalIntake.Count();
                getStudentTotalReportRowTotal.ExternalIntake.Value += getStudentExternalIntake.Count();
                getStudentTotalReportRowTotal.Inactive.Value += getStudentInactive.Count();
                getStudentTotalReportRowTotal.WithdrawalProcess.Value += getStudentWithdrawalProcess.Count();
                getStudentTotalReportRowTotal.Transfer.Value += getStudentTransfer.Count();
                getStudentTotalReportRowTotal.Active.Value += getStudentActive.Count();
                getStudentTotalReportRowTotal.Withdrawn.Value += getStudentWithdrawn.Count();
                getStudentTotalReportRowTotal.TotalStudents.Value += getStudentTotalStudents;

                getStudentTotalReportListDate.Add(getStudentTotalReportDate);
            }

            getStudentTotalReportSemestersData.Semester = paramSmt.ToString();
            getStudentTotalReportSemestersData.ListData = getStudentTotalReportListDate;
            getStudentTotalReportSemestersData.RowTotal = getStudentTotalReportRowTotal;

            return getStudentTotalReportSemestersData;
        }

        public async Task<List<GetSDRTotalStudentReportDetailsResult>> SetSDRTotalStudentDetails(int paramSmt, StudentDemographicsReportRequest param)
        {
            var GetSDRTotalStudentReportDetailList = new List<GetSDRTotalStudentReportDetailsResult>();

            var GetDataHomeroomFromStudentDemographicsReport = await SetHomeroomData(param, paramSmt);

            var getAccessByHomeroom = GetDataHomeroomFromStudentDemographicsReport
                    .Select(x => new
                    {
                        IdGrade = x.IdGrade,
                        GradeName = x.GradeName,
                        GradeCode = x.GradeCode,
                        IdHomeroom = x.IdHomeroom,
                        HomeroomName = x.HomeroomName
                    })
                        .OrderBy(a => a.GradeName.Length)
                        .ThenBy(a => a.GradeName)
                        .ThenBy(x => x.HomeroomName.Length)
                        .ThenBy(x => x.HomeroomName)
                    .Distinct().ToList();

            var homeroomData = await SetDataHomeroomAdmission(param, paramSmt);

            var studentGradePathwayData = await _dbContext.Entity<MsStudentGradePathway>()
                .Where(x => x.StudentGrade.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear &&
                            (param.Grade == null || param.Grade.Contains(x.StudentGrade.IdGrade)))
                .Select(x => new
                {
                    IdGrade = x.StudentGrade.IdGrade,
                    IdStudent = x.StudentGrade.IdStudent,
                    IdPathway = x.IdPathway,
                    Pathway = x.Pathway != null && x.Pathway.Code != null && x.Pathway.Code.ToLower() == "no pathway" ? "-" :
                              (x.Pathway != null && x.Pathway.Code != null ? x.Pathway.Code.ToLower() : "-")
                })
                .Distinct()
                .ToListAsync();

            var homeroomTeacherList = await _dbContext.Entity<MsHomeroomTeacher>()
                .Where(x => x.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear &&
                            x.IsShowInReportCard == true &&
                            (param.Grade == null || param.Grade.Contains(x.Homeroom.IdGrade)) &&
                            (param.Homeroom == null || param.Homeroom.Contains(x.Homeroom.Id))
                            )
                .Select(x => new
                {
                    IdHomeroom = x.IdHomeroom,
                    IdBinusian = x.IdBinusian,
                    TeacherName = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName),
                    Semester = x.Homeroom.Semester
                })
                .Distinct()
                .ToListAsync();

            foreach (var item in getAccessByHomeroom)
            {
                var homeroomAdmissionData = homeroomData
                    .Where(x => x.IdGrade == item.IdGrade &&
                                x.IdHomeroom == item.IdHomeroom &&
                                x.Semester == paramSmt)
                    .ToList();

                var getPeriodStartDate = GetDataHomeroomFromStudentDemographicsReport
                            .Where(x => x.IdGrade == item.IdGrade)
                            .OrderBy(x => x.Semester)
                            .ThenBy(x => x.StartDate)
                            .Select(x => x.StartDate)
                            .FirstOrDefault();

                var getHomeroomStudentAdmissionWithStatusType = await SetHomeroomStudentAdmissionWithStatusType(homeroomAdmissionData, param.IdType, getPeriodStartDate);

                foreach (var itemStudent in getHomeroomStudentAdmissionWithStatusType)
                {
                    var homeroomTeacher = homeroomTeacherList.FirstOrDefault(x => x.IdHomeroom == itemStudent.IdHomeroom && x.Semester == paramSmt);
                    var streaming = studentGradePathwayData.FirstOrDefault(x => x.IdGrade == itemStudent.IdGrade && x.IdStudent == itemStudent.IdStudent);

                    var studentDetail = new GetSDRTotalStudentReportDetailsResult
                    {
                        Student = new ItemValueVm { Id = itemStudent.IdStudent, Description = itemStudent.StudentName },
                        Level = new ItemValueVm { Id = itemStudent.IdLevel, Description = itemStudent.LevelName },
                        Grade = new ItemValueVm { Id = itemStudent.IdGrade, Description = itemStudent.GradeName },
                        Homeroom = new ItemValueVm { Id = itemStudent.IdHomeroom, Description = itemStudent.HomeroomName },
                        HomeroomTeacher = new ItemValueVm { Id = homeroomTeacher?.IdBinusian ?? "-", Description = homeroomTeacher?.TeacherName ?? "-"},
                        Streaming = streaming != null ? new ItemValueVm { Id = streaming.IdPathway, Description = streaming.Pathway } : null,
                        JoinToSchoolDate = itemStudent.JoinToSchoolDate?.ToString("dd MMMM yyyy") ?? "-"
                    };

                    GetSDRTotalStudentReportDetailList.Add(studentDetail);
                }
            }

            return GetSDRTotalStudentReportDetailList;
        }

        public List<StudentDemographicsReportResult_HomeroomStudentAdmissionData> getHomeroomStudentAdmissionStudentStatus(List<StudentDemographicsReportResult_HomeroomStudentAdmissionData> HomeroomStudentAdmissionData, int idStudentStatus)
        {
            return HomeroomStudentAdmissionData.Where(x => x.IdStudentStatus == idStudentStatus).ToList();
        }

        public List<StudentDemographicsReportResult_HomeroomStudentAdmissionData> getHomeroomStudentAdmissionStudentStatusActive(List<StudentDemographicsReportResult_HomeroomStudentAdmissionData> HomeroomStudentAdmissionData, int active, int graduate, int withdrawHidden)
        {
            return HomeroomStudentAdmissionData.Where(x => x.IdStudentStatus == active || x.IdStudentStatus == graduate || x.IdStudentStatus == withdrawHidden).ToList();
        }

        public async Task<List<StudentDemographicsReportResult_HomeroomStudentAdmissionData>> SetHomeroomStudentAdmissionWithStatusType(List<StudentDemographicsReportResult_HomeroomStudentAdmissionData> HomeroomStudentAdmissionData, string? IdType, DateTime? getPeriodStartDate)
        {
            var filterFunctions = new Dictionary<string, Func<List<StudentDemographicsReportResult_HomeroomStudentAdmissionData>, DateTime?, List<StudentDemographicsReportResult_HomeroomStudentAdmissionData>>>()
            {
                { _IdTypeInternalIntake, (data, startDate) => data.Where(x => (x.GradeCode != "EY1" && x.GradeCode != "ECY1") && x.JoinToSchoolDate == null).ToList() },
                { _IdTypeExternalIntake, (data, startDate) => data.Where(x => x.IdStudentAdmission != null && x.JoinToSchoolDate <= startDate).ToList() },
                { _IdTypeInactive, (data, startDate) => getHomeroomStudentAdmissionStudentStatus(data, 15) },
                { _IdTypeWithdrawalProcess, (data, startDate) => getHomeroomStudentAdmissionStudentStatus(data, 13) },
                { _IdTypeTransfer, (data, startDate) => data.Where(x => x.IdStudentAdmission != null && x.JoinToSchoolDate > startDate).ToList() },
                { _IdTypeActive, (data, startDate) => getHomeroomStudentAdmissionStudentStatusActive(data, 1, 7, 16) },
                { _IdTypeWithdrawn, (data, startDate) => getHomeroomStudentAdmissionStudentStatus(data, 5) }
            };

            if (IdType != null && filterFunctions.TryGetValue(IdType, out var filterFunction))
            {
                return filterFunction(HomeroomStudentAdmissionData, getPeriodStartDate);
            }
            else
            {
                return HomeroomStudentAdmissionData;
            }
        }
    }
}
