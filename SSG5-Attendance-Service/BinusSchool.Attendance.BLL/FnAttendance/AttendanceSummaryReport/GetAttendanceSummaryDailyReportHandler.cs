using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.DataByPosition;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryReport;
using BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition;
using BinusSchool.Persistence.AttendanceDb;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryReport
{
    public class GetAttendanceSummaryDailyReportHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly GetHomeroomTeacherPrivilegeHandler _getHomeroomTeacherPrivilegeHandler;

        public GetAttendanceSummaryDailyReportHandler(
            DbContextOptions<AttendanceDbContext> options,
            GetHomeroomTeacherPrivilegeHandler getHomeroomTeacherPrivilegeHandler
            )
        {
            _dbContext = new AttendanceDbContext(options);
            _getHomeroomTeacherPrivilegeHandler = getHomeroomTeacherPrivilegeHandler;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDailyReportRequest>(
                    nameof(GetAttendanceSummaryDailyReportRequest.IdUser),
                    nameof(GetAttendanceSummaryDailyReportRequest.IdSchool),
                    nameof(GetAttendanceSummaryDailyReportRequest.IdAcademicYear),
                    nameof(GetAttendanceSummaryDailyReportRequest.AttendanceDate),
                    nameof(GetAttendanceSummaryDailyReportRequest.Levels)
                    );

            var retVal = await GetAttendanceSummaryDailyReport(param);

            return Request.CreateApiResult2(retVal as object);
        }

        public async Task<GetAttendanceSummaryDailyReportResult> GetAttendanceSummaryDailyReport(GetAttendanceSummaryDailyReportRequest param)
        {
            var retVal = new GetAttendanceSummaryDailyReportResult();

            var homeroomPrivilege = await _getHomeroomTeacherPrivilegeHandler.GetHomeroomTeacherPrivileges(new GetHomeroomTeacherPrivilegeRequest()
            {
                IdAcademicYear = param.IdAcademicYear,
                IdSchool = param.IdSchool,
                IdUser = param.IdUser,
                IncludeClassAdvisor = false,
                IncludeSubjectTeacher = false,
            });

            var AllAccessLevel = homeroomPrivilege.Where(x => param.Levels.Any(y => y == x.IdLevel)).Select(x => x.IdLevel).Distinct().ToList();
            var AllAccessGrade = homeroomPrivilege.Select(x => x.IdGrade).Distinct().ToList();
            var AllAccessHomeroom = homeroomPrivilege.Select(x => x.IdHomeroom).Distinct().ToList();

            if (AllAccessLevel.Count() >= 1)
                param.Levels = AllAccessLevel;

            var getActiveSemester = await GetActiveSemester(param);
            var getTransactionAttendanceEntryV2 = await GetTransactionAttendanceEntryV2(param, getActiveSemester, AllAccessHomeroom);
            var getStudentEnrollment = await GetHomeroomStudentEnrollment(param, getActiveSemester, AllAccessHomeroom);
            var getJoinEnrollmentAndAttandance = GetDataEnrollmentAndAttandance(getTransactionAttendanceEntryV2, getStudentEnrollment, param, getActiveSemester, AllAccessHomeroom);

            if (getJoinEnrollmentAndAttandance.Count() == 0)
                return retVal = null;

            var getAnomaliDataAttandance = GetListAnomaliDataAttandance(getTransactionAttendanceEntryV2);
            var getDataTeacher = await GetDataTeacher(param, getAnomaliDataAttandance, getJoinEnrollmentAndAttandance);
            var getTappingTransaction = await GetDataTappingTransaction(param);

            var getStudentUAPresent = GetListUAPresentCheck(getAnomaliDataAttandance, getJoinEnrollmentAndAttandance, getDataTeacher, getTappingTransaction);
            var getStudentUAPresentOrderBySessionName = GetStudentUAPresentOrderBySessionName(getStudentUAPresent);

            //ListUAStudent
            var getUAStudents_ListUAStudent = GetListUAStudent(getStudentEnrollment, getAnomaliDataAttandance);
            //var getTeachersNotFilledAttendance = await GetTeachersNotFilledAttendance(getStudentUAPresentOrderBySessionName);
            var getTeachersNotFilledAttendance = GetTeachersNotFilledAttendanceAll(getJoinEnrollmentAndAttandance, getDataTeacher);
            var getListNotTapIn = GetlistNotTapIn(getStudentUAPresent);
            //listUAPresentCheck
            var getStudentUAPresentDetail = GetListUAPresentDetail(getStudentUAPresentOrderBySessionName);
            var getStudentUAPresentList = GetStudentUAPresentList(getStudentUAPresentOrderBySessionName);
            //Summary
            var getSummaryDetail = GetSummaryDetail(getStudentUAPresentOrderBySessionName);
            var getSummaryPercentPerSession = GetSummaryPercentPerSession(getSummaryDetail);
            var getSummaryPerGradeSession = GetSummaryPerGradeSession(getSummaryDetail);

            if (getStudentUAPresentOrderBySessionName.Count() == 0)
                return retVal;

            retVal = new GetAttendanceSummaryDailyReportResult()
            {
                UAStudents = new GetAttendanceSummaryDailyReportResult_UAStudents()
                {
                    ListUAStudent = getUAStudents_ListUAStudent,
                    ListTeacherAttendance = getTeachersNotFilledAttendance,
                    ListNotTapIn = getListNotTapIn
                },
                UAPresentStudent = new GetAttendanceSummaryDailyReportResult_UAPresentStudent()
                {
                    ListUAPresentCheck = getStudentUAPresentDetail,
                    ListDetail = getStudentUAPresentList
                },
                Summary = new GetAttendanceSummaryDailyReportResult_Summary()
                {
                    ListPercentPerSession = getSummaryPercentPerSession,
                    ListSummaryPerGradeSession = getSummaryPerGradeSession,
                    ListDetail = getSummaryDetail
                }
            };

            return retVal;
        }

        public async Task<int> GetActiveSemester(GetAttendanceSummaryDailyReportRequest param)
        {
            var retVal = await _dbContext.Entity<MsPeriod>()
                    .Where(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear &&
                                x.StartDate.Date <= param.AttendanceDate.Date && param.AttendanceDate.Date <= x.EndDate.Date)
                    //.Where(x => param.Levels.Any(y => y == x.Grade.IdLevel))
                    .Select(x => x.Semester)
                    .FirstOrDefaultAsync(CancellationToken);

            return retVal;
        }

        public async Task<List<GetAttendanceSummaryDailyReportResult_DataTeacher>> GetDataTeacher(GetAttendanceSummaryDailyReportRequest param, List<GetAttendanceSummaryDailyReportResult_JoinEnrollmentAndAttandance> getAnomaliDataAttandance, List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandance> getJoinEnrollmentAndAttandance)
        {
            var getIdLessonAnomaliData = getAnomaliDataAttandance.SelectMany(x => x.Attandance).Select(x => x.IdLesson).Distinct().ToList();
            var getIdLessonEnrollment = getJoinEnrollmentAndAttandance.Select(x => x.IdLesson).Distinct().ToList();

            var IdLessonList = getIdLessonAnomaliData.Union(getIdLessonEnrollment).Distinct().OrderBy(x => x).ToList();

            var getDataTeacher = await _dbContext.Entity<MsLessonTeacher>()
                    .Where(x => x.IsPrimary == true)
                    .Where(x => IdLessonList.Contains(x.IdLesson))
                    .Select(x => new {
                        ClassIdGenerated = x.Lesson.ClassIdGenerated,
                        IdLesson = x.IdLesson,
                        IdTeacher = x.IdUser,
                        TeacherName = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                    }).Distinct()
                    .ToListAsync(CancellationToken);

            var retVal = getDataTeacher
                    .Select(x => new GetAttendanceSummaryDailyReportResult_DataTeacher
                    {
                        ClassIdGenerated = x.ClassIdGenerated,
                        IdLesson = x.IdLesson,
                        IdTeacher = x.IdTeacher,
                        TeacherName = x.TeacherName
                    }).ToList();

            return retVal;
        }

        public async Task<List<GetAttendanceSummaryDailyReportResult_DataTappingTransaction>> GetDataTappingTransaction(GetAttendanceSummaryDailyReportRequest param)
        {
            var getTappingTransaction = await _dbContext.Entity<TrTappingTransaction>()
                    .Where(x => x.TransactionTime.Date == param.AttendanceDate.Date)
                    .Select(x => new {
                        IdBinusianCard = x.Card.BinusianID.Trim(),
                        IdBinusianTap = x.Binusian_id.Trim(),
                        CardIDCard = x.Card.CardID.Trim(),
                        CardIDTap = x.CardUID.Trim(),
                        TransactionTime = x.TransactionTime,
                        IsActive = x.Card.IsActive
                    }).Distinct()
                    .ToListAsync(CancellationToken);

            var retVal = getTappingTransaction
                    .Select(x => new GetAttendanceSummaryDailyReportResult_DataTappingTransaction
                    {
                        IdBinusianCard = x.IdBinusianCard,
                        IdBinusianTap = x.IdBinusianTap,
                        CardIDCard = x.CardIDCard,
                        CardIDTap = x.CardIDTap,
                        TransactionTime = x.TransactionTime,
                        IsActive = x.IsActive,
                    }).ToList();

            return retVal;
        }

        public async Task<List<GetAttendanceSummaryDailyReportResult_DataTransactionAttandance>> GetTransactionAttendanceEntryV2(GetAttendanceSummaryDailyReportRequest param, int activeSemester, List<string> AllAccessHomeroom)
        {
            var getTransactionAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                    .Where(x => x.ScheduleLesson.ScheduleDate.Date == param.AttendanceDate.Date &&
                                x.ScheduleLesson.IdAcademicYear == param.IdAcademicYear &&
                                x.ScheduleLesson.Lesson.Semester == activeSemester &&
                                x.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear &&
                                x.HomeroomStudent.Homeroom.Semester == activeSemester &&
                                x.AttendanceMappingAttendance.Attendance.IdAcademicYear == param.IdAcademicYear)
                    //.Where(x => !x.HomeroomStudent.Student.TrStudentStatuss.Any() || (x.HomeroomStudent.Student.TrStudentStatuss.Any() && x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.CurrentStatus == "A" && a.IdStudentStatus == 1)))
                    .Where(x => param.Levels.Any(y => y == x.ScheduleLesson.IdLevel))
                    .Where(x => param.Levels.Any(y => y == x.HomeroomStudent.Homeroom.Grade.IdLevel))
                    .Where(x => param.Levels.Any(y => y == x.AttendanceMappingAttendance.MappingAttendance.IdLevel))
                    .Select(x => new {
                        IdHomeroom = x.HomeroomStudent.IdHomeroom,
                        SessionID = x.ScheduleLesson.SessionID,
                        SessionName = x.ScheduleLesson.Session.Name,
                        SessionAlias = x.ScheduleLesson.Session.Alias,
                        IdTeacher = x.IdBinusian,
                        IdBinusian = x.HomeroomStudent.Student.IdBinusian.Trim(),
                        IdStudent = x.HomeroomStudent.Student.Id.Trim(),
                        ClassID = x.ScheduleLesson.Lesson.ClassIdGenerated,
                        IdLesson = x.ScheduleLesson.IdLesson,
                        AttendanceCategory = x.AttendanceMappingAttendance.Attendance.AttendanceCategory,
                        AbsenceCategory = x.AttendanceMappingAttendance.Attendance.AbsenceCategory,
                        AttendanceDesc = x.AttendanceMappingAttendance.Attendance.Description,
                        DateIn = x.DateIn
                    }).Distinct()
                    .OrderBy(x => x.SessionID)
                    .ToListAsync(CancellationToken);

            var retVal = getTransactionAttendanceEntry
                    .Where(x => AllAccessHomeroom.Any(y => y == x.IdHomeroom))
                    .Select(x => new GetAttendanceSummaryDailyReportResult_DataTransactionAttandance
                    {
                        SessionID = x.SessionID,
                        SessionName = x.SessionName,
                        SessionAlias = x.SessionAlias,
                        IdTeacher = x.IdTeacher,
                        IdBinusian = x.IdBinusian,
                        IdStudent = x.IdStudent,
                        ClassID = x.ClassID,
                        IdLesson = x.IdLesson,
                        AttendanceCategory = x.AttendanceCategory,
                        AbsenceCategory = x.AbsenceCategory,
                        AttendanceDesc = x.AttendanceDesc,
                        DateIn = x.DateIn,
                    }).ToList();

            return retVal;
        }

        public async Task<List<GetAttendanceSummaryDailyReportResult_DataEnrollment>> GetHomeroomStudentEnrollment(GetAttendanceSummaryDailyReportRequest param, int activeSemester, List<string> AllAccessHomeroom)
        {
            var retVal = new List<GetAttendanceSummaryDailyReportResult_DataEnrollment>();

            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                    .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                    .Select(x => new { x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus })
                    .Where(x => ( (x.StartDate == param.AttendanceDate.Date || x.EndDate == param.AttendanceDate.Date || (x.StartDate < param.AttendanceDate.Date)
                        ? x.EndDate != null 
                             ? (x.EndDate > param.AttendanceDate.Date && x.EndDate < param.AttendanceDate.Date) || x.EndDate > param.AttendanceDate.Date 
                             : x.StartDate <= param.AttendanceDate.Date
                        : x.EndDate != null 
                             ? ((param.AttendanceDate.Date > x.StartDate && param.AttendanceDate.Date < x.EndDate) || param.AttendanceDate.Date > x.EndDate) 
                             : x.StartDate <= param.AttendanceDate.Date)) 
                        && x.CurrentStatus == "A" 
                        && x.ActiveStatus == false)
                    .ToListAsync();

            var getStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                    .Where(x => x.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear &&
                                x.HomeroomStudent.Homeroom.Semester == activeSemester)
                    .Where(x => AllAccessHomeroom.Any(y => y == x.HomeroomStudent.IdHomeroom))
                    //.Where(x => param.Levels.Any(y => y == x.HomeroomStudent.Homeroom.Grade.IdLevel))
                    //.Where(x => !x.HomeroomStudent.Student.TrStudentStatuss.Any() || (x.HomeroomStudent.Student.TrStudentStatuss.Any() && x.HomeroomStudent.Student.TrStudentStatuss.Any(a => a.CurrentStatus == "A" && (a.ActiveStatus == true || a.IdTrStudentStatus == null || a.IdStudentStatus == 7))))
                    .Select(x => new
                    {
                        IdBinusian = x.HomeroomStudent.Student.IdBinusian.Trim(),
                        IdStudent = x.HomeroomStudent.Student.Id.Trim(),
                        StudentName = NameUtil.GenerateFullName(x.HomeroomStudent.Student.FirstName, x.HomeroomStudent.Student.LastName),
                        IdLevel = x.HomeroomStudent.Homeroom.Grade.IdLevel,
                        IdGrade = x.HomeroomStudent.Homeroom.IdGrade,
                        GradeCode = x.HomeroomStudent.Homeroom.Grade.Code,
                        IdHomeroom = x.HomeroomStudent.IdHomeroom,
                        HomeroomCode = x.HomeroomStudent.Homeroom.Grade.Code + "" + Regex.Replace(x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code, @"[^a-zA-Z0-9]", string.Empty),
                        ClassCode = Regex.Replace(x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code, @"[^a-zA-Z0-9]", string.Empty),
                        ClassID = x.Lesson.ClassIdGenerated,
                        IdLesson = x.IdLesson
                    }).Distinct()
                    .ToListAsync(CancellationToken);

            /*
            * Status S/E apabila:
            * - Expelled           (2)
            * - Transfer (Resign)  (5)
            * - Pass Away          (9)
            */
            int[] studentExitStatus = { 2, 5, 9 };

            foreach (var data in getStudentEnrollment)
            {
                var getStudentStatus = checkStudentStatus
                        .Where(x => x.IdStudent == data.IdStudent)
                        .FirstOrDefault();

                var IsExitStudent = getStudentStatus == null ? false : studentExitStatus.Contains(getStudentStatus.IdStudentStatus);

                if (!IsExitStudent)
                {
                    var studentStatus = new GetAttendanceSummaryDailyReportResult_DataEnrollment
                    {
                        IdBinusian = data.IdBinusian,
                        IdStudent = data.IdStudent,
                        StudentName = data.StudentName,
                        IdLevel = data.IdLevel,
                        IdGrade = data.IdGrade,
                        GradeCode = data.GradeCode,
                        IdHomeroom = data.IdHomeroom,
                        HomeroomCode = data.HomeroomCode,
                        ClassCode = data.ClassCode,
                        ClassID = data.ClassID,
                        IdLesson = data.IdLesson,
                    };
                    retVal.Add(studentStatus);
                }
            }

            return retVal;
        }

        public List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandance> GetDataEnrollmentAndAttandance(List<GetAttendanceSummaryDailyReportResult_DataTransactionAttandance> getDataTransactionAttandance, List<GetAttendanceSummaryDailyReportResult_DataEnrollment> getDataEnrollment, GetAttendanceSummaryDailyReportRequest param, int activeSemester, List<string> AllAccessHomeroom)
        {
            var getScheduleLesson = _dbContext.Entity<MsScheduleLesson>()
                    .Where(x => x.IdAcademicYear == param.IdAcademicYear &&
                                x.Lesson.Semester == activeSemester &&
                                x.ScheduleDate.Date == param.AttendanceDate.Date)
                    .Where(x => param.Levels.Any(y => y == x.IdLevel))
                    .Select(x => new
                    {
                        IdLesson = x.IdLesson,
                        ClassID = x.ClassID,
                        IdSession = x.IdSession,
                        SessionID = x.SessionID,
                        SessionName = x.Session.Name,
                        SessionAlias = x.Session.Alias,
                    }).Distinct().ToList();

            var JoinScheduleEnrollment = getScheduleLesson
                    .Join(
                        getDataEnrollment,
                    schedule => schedule.IdLesson,
                    student => student.IdLesson,
                    (schedule, enrollment) => new
                    {
                        IdLesson = schedule.IdLesson,
                        ClassID = schedule.ClassID,
                        IdSession = schedule.IdSession,
                        SessionID = schedule.SessionID,
                        SessionName = schedule.SessionName,
                        SessionAlias = schedule.SessionAlias,
                        IdStudent = enrollment.IdStudent,
                        IdBinusian = enrollment.IdBinusian,
                        StudentName = enrollment.StudentName,
                        GradeCode = enrollment.GradeCode,
                        IdHomeroom = enrollment.IdHomeroom,
                        HomeroomCode = enrollment.HomeroomCode,
                        ClassCode = enrollment.ClassCode,
                    }).Distinct()
                    .ToList();

            var getAllEnrollmentAttandanceStudent = JoinScheduleEnrollment
                    .GroupJoin(getDataTransactionAttandance,
                        c => (c.IdBinusian, c.IdStudent, c.SessionID),
                        s => (s.IdBinusian, s.IdStudent, s.SessionID),
                        (c1, t1) => new { c = c1, ts = t1 }
                    ).SelectMany(c1 => c1.ts.DefaultIfEmpty(),
                    (c1, t1) => new
                    {
                        IdLesson = c1.c.IdLesson,
                        ClassID = c1.c.ClassID,
                        IdSession = c1.c.IdSession,
                        SessionID = c1.c.SessionID,
                        SessionName = c1.c.SessionName,
                        SessionAlias = c1.c.SessionAlias,
                        IdStudent = c1.c.IdStudent,
                        IdBinusian = c1.c.IdBinusian,
                        StudentName = c1.c.StudentName,
                        GradeCode = c1.c.GradeCode,
                        IdHomeroom = c1.c.IdHomeroom,
                        HomeroomCode = c1.c.HomeroomCode,
                        ClassCode = c1.c.ClassCode,
                        IdTeacher = t1?.IdTeacher,
                        AttendanceCategory = t1?.AttendanceCategory,
                        AbsenceCategory = t1?.AbsenceCategory,
                        AttendanceDesc = t1?.AttendanceDesc,
                    }).Distinct().ToList();

            var retVal = getAllEnrollmentAttandanceStudent
                    .Where(x => AllAccessHomeroom.Any(y => y == x.IdHomeroom))
                    .Select(x => new GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandance
                    {
                        IdLesson = x.IdLesson,
                        ClassID = x.ClassID,
                        IdSession = x.IdSession,
                        SessionID = x.SessionID,
                        SessionName = x.SessionName,
                        SessionAlias = x.SessionAlias,
                        IdStudent = x.IdStudent,
                        IdBinusian = x.IdBinusian,
                        StudentName = x.StudentName,
                        GradeCode = x.GradeCode,
                        IdHomeroom = x.IdHomeroom,
                        HomeroomCode = x.HomeroomCode,
                        ClassCode = x.ClassCode,
                        IdTeacher = x.IdTeacher,
                        AttendanceCategory = x.AttendanceCategory,
                        AbsenceCategory = x.AbsenceCategory,
                        AttendanceDesc = x.AttendanceDesc,
                    }).ToList();

            return retVal;
        }

        public List<GetAttendanceSummaryDailyReportResult_Student> GetListUAStudent(List<GetAttendanceSummaryDailyReportResult_DataEnrollment> getDataEnrollment, List<GetAttendanceSummaryDailyReportResult_JoinEnrollmentAndAttandance> getAnomaliDataAttandance)
        {
            var retVal = new List<GetAttendanceSummaryDailyReportResult_Student>();

            var getIdStudentPureUA = getAnomaliDataAttandance.Where(x => x.IsCheck == false && x.Attandance.All(x => x.AttendanceDesc == "Absent")).Select(x => x.IdStudent).Distinct().ToList();

            foreach (var student in getIdStudentPureUA)
            {
                var getStudentNameAndClass = getDataEnrollment.Where(x => x.IdStudent == student).FirstOrDefault();

                if(getStudentNameAndClass != null)
                {
                    var getListUAStudent = new GetAttendanceSummaryDailyReportResult_Student()
                    {
                        Student = new NameValueVm
                        {
                            Id = getStudentNameAndClass.IdStudent,
                            Name = getStudentNameAndClass.StudentName
                        },
                        Homeroom = new ItemValueVm
                        {
                            Id = getStudentNameAndClass.GradeCode,
                            Description = getStudentNameAndClass.HomeroomCode
                        }
                    };
                    retVal.Add(getListUAStudent);
                }
            }

            return retVal.OrderBy(x => int.Parse(x.Homeroom.Id)).ThenBy(x => x.Homeroom.Description).ThenBy(x => x.Student.Name).ToList();
        }

        public List<GetAttendanceSummaryDailyReportResult_Student> GetlistNotTapIn(List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandance> getDataEnrollmentAndAttandance)
        {
            var retVal = new List<GetAttendanceSummaryDailyReportResult_Student>();
            var getListNotTapIn = getDataEnrollmentAndAttandance
                    .Select(x => new {
                        TappingTime = x.TappingTime,
                        IdStuden = x.IdStudent,
                        StudentName = x.StudentName,
                        GradeCode = x.GradeCode,
                        HomeroomName = x.HomeroomCode
                    }).Distinct()
                    .ToList();

            retVal = getListNotTapIn
                    .Where(x => x.TappingTime == null)
                    .Select(x => new GetAttendanceSummaryDailyReportResult_Student
                    {
                        Student = new NameValueVm
                        {
                            Id = x.IdStuden,
                            Name = x.StudentName,
                        },
                        Homeroom = new ItemValueVm
                        {
                            Id = x.GradeCode,
                            Description = x.HomeroomName
                        }
                    }).ToList();

            return retVal.OrderBy(x => int.Parse(x.Homeroom.Id)).ThenBy(x => x.Homeroom.Description).ThenBy(x => x.Student.Name).ToList();
        }

        public List<GetAttendanceSummaryDailyReportResult_JoinEnrollmentAndAttandance> GetListAnomaliDataAttandance(List<GetAttendanceSummaryDailyReportResult_DataTransactionAttandance> getDataTransactionAttandance)
        {
            var retVal = getDataTransactionAttandance
                   .GroupBy(x => new { x.IdStudent, x.IdBinusian })
                   .Select(group => new GetAttendanceSummaryDailyReportResult_JoinEnrollmentAndAttandance
                   {
                       IdStudent = group.Key.IdStudent,
                       IdBinusian = group.Key.IdBinusian,
                       IsCheck = (group.Any(x => x.AttendanceCategory == AttendanceCategory.Present) && group.Any(x => x.AttendanceDesc == "Absent")),
                       Attandance = group.ToList()
                   }).ToList();

            return retVal.OrderBy(x => x.IdStudent).ToList();
        }

        public List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandance> GetListUAPresentCheck(List<GetAttendanceSummaryDailyReportResult_JoinEnrollmentAndAttandance> getAnomaliDataAttandance, List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandance> getDataEnrollmentAndAttandance, List<GetAttendanceSummaryDailyReportResult_DataTeacher> getDataTeacher, List<GetAttendanceSummaryDailyReportResult_DataTappingTransaction> getTappingTransaction)
        {
            var retVal = new List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandance>();

            var getAnomaliIdStudent = getAnomaliDataAttandance
                   .Where(x => x.IsCheck == true)
                   .Select(x => new { x.IdStudent, x.IdBinusian })
                   .Distinct()
                   .OrderBy(x => x.IdStudent)
                   .ToList();

            foreach (var student in getAnomaliIdStudent)
            {
                var SessionIdList = new List<string>();

                var getAnomaliDataStudent = getAnomaliDataAttandance
                        .Where(x => x.IdStudent == student.IdStudent)
                        .SelectMany(x => x.Attandance)
                        .ToList();

                var getStudentScheduleEnrollment = getDataEnrollmentAndAttandance
                        .Where(x => x.IdStudent == student.IdStudent)
                        .ToList();

                var getSessionIdAnomaliData = getAnomaliDataStudent.Select(x => x.SessionID).Distinct().ToList();
                var getSessionIdEnrollment = getStudentScheduleEnrollment.Select(x => x.SessionID).Distinct().ToList();

                SessionIdList = getSessionIdAnomaliData.Union(getSessionIdEnrollment).Distinct().OrderBy(x => x).ToList();

                foreach (var SessionId in SessionIdList)
                {
                    var getAnomaliDataByStudent = new GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandance();

                    DateTime? getTappingTime = getTappingTransaction
                            .Where(x => x.IdBinusianTap == student.IdBinusian || x.IdBinusianCard == student.IdBinusian || x.IdBinusianCard == student.IdStudent || x.IdBinusianTap == student.IdStudent)
                            .Select(x => x.TransactionTime)
                            .FirstOrDefault();

                    var checkSessinIdInAnomaliData = getSessionIdAnomaliData.Contains(SessionId);
                    var checkSessinIdInEnrollment = getSessionIdEnrollment.Contains(SessionId);

                    var getAnomaliDataBySessionId = getAnomaliDataStudent
                            .Where(x => x.SessionID == SessionId)
                            .OrderByDescending(x => x.DateIn)
                            .FirstOrDefault();

                    var getEnrollmentBySessionId = getStudentScheduleEnrollment
                            .Where(x => x.SessionID == SessionId)
                            .FirstOrDefault();

                    if (!checkSessinIdInEnrollment && checkSessinIdInAnomaliData)
                    {
                        var getEnrollmentBySessionIdNoEnrollment = getStudentScheduleEnrollment
                                .FirstOrDefault();

                        var getTeacherDataNoEnrollment = getDataTeacher.Where(x => x.IdLesson == getAnomaliDataBySessionId.IdLesson).FirstOrDefault();

                        getAnomaliDataByStudent = new GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandance()
                        {
                            IdStudent = getEnrollmentBySessionIdNoEnrollment.IdStudent,
                            StudentName = getEnrollmentBySessionIdNoEnrollment.StudentName,
                            IdBinusian = getEnrollmentBySessionIdNoEnrollment.IdBinusian,
                            IdLesson = getEnrollmentBySessionIdNoEnrollment.IdLesson,
                            GradeCode = getEnrollmentBySessionIdNoEnrollment.GradeCode,
                            IdTeacher = getTeacherDataNoEnrollment.IdTeacher,
                            TeacherName = getTeacherDataNoEnrollment.TeacherName,
                            IdHomeroom = getEnrollmentBySessionIdNoEnrollment.IdHomeroom,
                            HomeroomCode = getEnrollmentBySessionIdNoEnrollment.HomeroomCode,
                            ClassCode = getEnrollmentBySessionIdNoEnrollment.ClassCode,
                            ClassID = getEnrollmentBySessionIdNoEnrollment.ClassID,
                            SessionID = getEnrollmentBySessionIdNoEnrollment.SessionID,
                            SessionName = getEnrollmentBySessionIdNoEnrollment.SessionName,
                            SessionAlias = getEnrollmentBySessionIdNoEnrollment.SessionAlias,
                            TappingTime = getTappingTime ?? null,
                            AttendanceCategory = getAnomaliDataBySessionId?.AttendanceCategory,
                            AbsenceCategory = getAnomaliDataBySessionId?.AbsenceCategory,
                            AttendanceDesc = getAnomaliDataBySessionId?.AttendanceDesc,
                        };
                        
                        retVal.Add(getAnomaliDataByStudent);
                    }
                    else
                    {
                        var getTeacherData = getDataTeacher.Where(x => x.IdLesson == getEnrollmentBySessionId.IdLesson).FirstOrDefault();

                        getAnomaliDataByStudent = new GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandance()
                        {
                            IdStudent = getEnrollmentBySessionId.IdStudent,
                            StudentName = getEnrollmentBySessionId.StudentName,
                            IdBinusian = getEnrollmentBySessionId.IdBinusian,
                            IdLesson = getEnrollmentBySessionId.IdLesson,
                            GradeCode = getEnrollmentBySessionId.GradeCode,
                            IdTeacher = getTeacherData.IdTeacher,
                            TeacherName = getTeacherData.TeacherName,
                            IdHomeroom = getEnrollmentBySessionId.IdHomeroom,
                            HomeroomCode = getEnrollmentBySessionId.HomeroomCode,
                            ClassCode = getEnrollmentBySessionId.ClassCode,
                            ClassID = getEnrollmentBySessionId.ClassID,
                            SessionID = getEnrollmentBySessionId.SessionID,
                            SessionName = getEnrollmentBySessionId.SessionName,
                            SessionAlias = getEnrollmentBySessionId.SessionAlias,
                            TappingTime = getTappingTime ?? null,
                            AttendanceCategory = getAnomaliDataBySessionId?.AttendanceCategory,
                            AbsenceCategory = getAnomaliDataBySessionId?.AbsenceCategory,
                            AttendanceDesc = getAnomaliDataBySessionId?.AttendanceDesc,
                        };

                        retVal.Add(getAnomaliDataByStudent);
                    }
                }

                var selectIdStudent = retVal.Where(x => x.IdStudent == student.IdStudent).ToList();
            }

            return retVal.OrderBy(x => x.GradeCode).ThenBy(x => x.HomeroomCode).ThenBy(x => x.StudentName).ToList();
        }

        public List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandanceBySessionName> GetStudentUAPresentOrderBySessionName(List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandance> getDataEnrollmentAndAttandance)
        {
            var retVal = new List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandanceBySessionName>();

            if (getDataEnrollmentAndAttandance.Count() == 0)
                return retVal;

            int number = 0;
            var selected = getDataEnrollmentAndAttandance?.Select(x => x.SessionAlias).Where(x => int.TryParse(x, out number)).Select(x => Int32.Parse(x)).Distinct().OrderBy(x => x).ToList();
            var getLatestSession = selected.Max();

            var getAnomaliIdStudent = getDataEnrollmentAndAttandance
                   .Select(x => new { x.IdStudent, x.StudentName, x.IdBinusian, x.IdHomeroom, x.HomeroomCode, x.GradeCode })
                   .Distinct()
                   .OrderBy(x => x.GradeCode).ThenBy(x => x.HomeroomCode).ThenBy(x => x.StudentName)
                   .ToList();

            foreach (var data in getAnomaliIdStudent)
            {
                var dataStudent = getDataEnrollmentAndAttandance
                        .Where(x => x.IdStudent == data.IdStudent)
                        .OrderBy(x => x.SessionID)
                        .Distinct()
                        .ToList();

                var getStudentTapIn = dataStudent.Select(x => x.TappingTime).FirstOrDefault();

                for (int i = 0; i <= getLatestSession; i++)
                {
                    var dataStudentSession = dataStudent
                            .Where(x => x.SessionName == i.ToString())
                            .FirstOrDefault();

                    var dataStudentAttandance = new GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandanceBySessionName
                    {
                        IdBinusian = data.IdBinusian,
                        IdStudent = data.IdStudent,
                        StudentName = data.StudentName,
                        GradeCode = data.GradeCode,
                        IdHomeroom = data.IdHomeroom,
                        HomeroomCode = data.HomeroomCode,
                        ClassCode = data.HomeroomCode,
                        ClassID = dataStudentSession?.ClassID,
                        IdLesson = dataStudentSession?.IdLesson,
                        IdTeacher = dataStudentSession?.IdTeacher,
                        TeacherName = dataStudentSession?.TeacherName,
                        SessionID = dataStudentSession?.SessionID,
                        SessionName = i.ToString(),
                        AttendanceCode = "",
                        AttendanceDesc = "",
                        TappingTime = getStudentTapIn != null ? getStudentTapIn.Value.ToString("HH:mm") : ""
                    };

                    if (dataStudentSession != null)
                    {
                        if (dataStudentSession.AttendanceDesc == null)
                        {
                            dataStudentAttandance.AttendanceCode = "TBE";
                            dataStudentAttandance.AttendanceDesc = "TBE";
                        }
                        else if (dataStudentSession.AttendanceDesc == "Present")
                        {
                            dataStudentAttandance.AttendanceCode = dataStudentSession.AttendanceDesc;
                            dataStudentAttandance.AttendanceDesc = "&check;";
                        }
                        else if (dataStudentSession.AttendanceDesc == "Absent")
                        {
                            dataStudentAttandance.AttendanceCode = "UA";
                            dataStudentAttandance.AttendanceDesc = "UA";
                        }
                        else if (dataStudentSession.AttendanceDesc == "Late")
                        {
                            dataStudentAttandance.AttendanceCode = dataStudentSession.AttendanceDesc;
                            dataStudentAttandance.AttendanceDesc = "L";
                        }
                        else
                        {
                            dataStudentAttandance.AttendanceCode = "TBE";
                            dataStudentAttandance.AttendanceDesc = "&#88;";
                        }
                    }
                    else
                    {
                        dataStudentAttandance.AttendanceCode = "X";
                        dataStudentAttandance.AttendanceDesc = "X";
                    }

                    retVal.Add(dataStudentAttandance);
                }
            }
            return retVal;
        }

        public List<GetAttendanceSummaryDailyReportResult_StudentAttandance> GetListUAPresentDetail(List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandanceBySessionName> getDataEnrollmentAndAttandanceBySessionName)
        {
            var retVal = new List<GetAttendanceSummaryDailyReportResult_StudentAttandance>();

            var getAnomaliIdStudent = getDataEnrollmentAndAttandanceBySessionName
                   .Select(x => new { x.IdStudent, x.StudentName, x.IdBinusian, x.IdHomeroom, x.HomeroomCode, x.TappingTime, x.GradeCode })
                   .Distinct()
                   .OrderBy(x => x.GradeCode).ThenBy(x => x.HomeroomCode).ThenBy(x => x.StudentName)
                   .ToList();

            foreach (var data in getAnomaliIdStudent)
            {
                var getSessionAttandance = getDataEnrollmentAndAttandanceBySessionName
                        .Where(x => x.IdStudent == data.IdStudent)
                        .Distinct()
                        .ToList();

                var dataStudentAttandance = new GetAttendanceSummaryDailyReportResult_StudentAttandance
                {
                    Student = new NameValueVm
                    {
                        Id = data.IdStudent,
                        Name = data.StudentName,
                    },
                    Homeroom = new ItemValueVm
                    {
                        Id = data.GradeCode,
                        Description = data.HomeroomCode
                    },
                    TappingTime = data.TappingTime,
                    SessionAttandance = getSessionAttandance.Select(x => new CodeWithIdVm
                    {
                        Id = x.SessionName,
                        Code = x.AttendanceCode,
                        Description = x.AttendanceDesc
                    }).ToList(),
                    TotalPresent = getSessionAttandance.Where(x => x.AttendanceCode == "Present").Count(),
                    TotalAbsent = getSessionAttandance.Where(x => x.AttendanceCode == "UA").Count(),
                    TotalLate = getSessionAttandance.Where(x => x.AttendanceCode == "Late").Count(),
                };

                retVal.Add(dataStudentAttandance);
            }

            return retVal.OrderBy(x => int.Parse(x.Homeroom.Id)).ThenBy(x => x.Homeroom.Description).ThenBy(x => x.Student.Name).ToList();
        }

        public List<GetAttendanceSummaryDailyReportResult_StudentUAPresent> GetStudentUAPresentList(List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandanceBySessionName> getDataEnrollmentAndAttandanceBySessionName)
        {
            var retVal = new List<GetAttendanceSummaryDailyReportResult_StudentUAPresent>();

            foreach (var data in getDataEnrollmentAndAttandanceBySessionName.Where(x => x.AttendanceCode != "X").OrderBy(x => x.GradeCode).ThenBy(x => x.HomeroomCode).ThenBy(x => x.StudentName).ToList())
            {
                var dataStudentAttandance = new GetAttendanceSummaryDailyReportResult_StudentUAPresent
                {
                    Student = new NameValueVm
                    {
                        Id = data.IdStudent,
                        Name = data.StudentName,
                    },
                    Homeroom = new ItemValueVm
                    {
                        Id = data.GradeCode,
                        Description = data.HomeroomCode
                    },
                    Session = new CodeWithIdVm
                    {
                        Id = data.SessionName,
                        Code = data.AttendanceCode,
                        Description = data.AttendanceDesc
                    },
                    Teacher = new NameValueVm
                    {
                        Id = data.IdTeacher,
                        Name = data.TeacherName
                    },
                    ClassID = data.ClassID,
                };

                retVal.Add(dataStudentAttandance);
            }

            return retVal.OrderBy(x => int.Parse(x.Homeroom.Id)).ThenBy(x => x.Homeroom.Description).ThenBy(x => x.Student.Name).ToList();
        }

        public List<GetAttendanceSummaryDailyReportResult_Teacher> GetTeachersNotFilledAttendance(List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandanceBySessionName> getDataEnrollmentAndAttandanceBySessionName)
        {
            var retVal = getDataEnrollmentAndAttandanceBySessionName.OrderBy(x => x.GradeCode).ThenBy(x => x.HomeroomCode).ThenBy(x => x.StudentName)
                    .Where(x => x.AttendanceCode == "TBE")
                    .GroupBy(x => new { x.IdTeacher, x.TeacherName })
                    .Select(group => new GetAttendanceSummaryDailyReportResult_Teacher
                    {
                        Teacher = new NameValueVm
                        {
                            Id = group.Key.IdTeacher,
                            Name = group.Key.TeacherName,
                        },
                        SessionTeacher = string.Join(", ", group.Select(x => x.SessionName).Distinct().ToList()),
                    }).ToList();

            return retVal.OrderBy(x => x.Teacher.Name).ToList();
        }

        public List<GetAttendanceSummaryDailyReportResult_Teacher> GetTeachersNotFilledAttendanceAll(List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandance> getDataEnrollmentAndAttandanceBySessionName, List<GetAttendanceSummaryDailyReportResult_DataTeacher> getDataTeacher)
        {
            var DataTeacherNotFilledAttandance = getDataEnrollmentAndAttandanceBySessionName.OrderBy(x => x.GradeCode).ThenBy(x => x.HomeroomCode).ThenBy(x => x.StudentName)
                   .Where(x => x.AttendanceDesc == null)
                   .Distinct()
                   .ToList();

            foreach (var data in DataTeacherNotFilledAttandance)
            {
                var getTeacherDataNoEnrollment = getDataTeacher.Where(x => x.IdLesson == data.IdLesson).FirstOrDefault();
                data.IdTeacher = getTeacherDataNoEnrollment.IdTeacher;
                data.TeacherName = getTeacherDataNoEnrollment.TeacherName;
            }

            var retVal = DataTeacherNotFilledAttandance.OrderBy(x => x.GradeCode).ThenBy(x => x.HomeroomCode).ThenBy(x => x.StudentName)
                    .GroupBy(x => new { x.IdTeacher, x.TeacherName })
                    .Select(group => new GetAttendanceSummaryDailyReportResult_Teacher
                    {
                        Teacher = new NameValueVm
                        {
                            Id = group.Key.IdTeacher,
                            Name = group.Key.TeacherName,
                        },
                        SessionTeacher = string.Join(", ", group.Select(x => x.SessionName).Distinct().ToList()),
                    }).ToList();

            return retVal.OrderBy(x => x.Teacher.Name).ToList();
        }

        public List<GetAttendanceSummaryDailyReportResult_SummaryStudentUA> GetSummaryDetail(List<GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandanceBySessionName> getDataEnrollmentAndAttandanceBySessionName)
        {
            var retVal = new List<GetAttendanceSummaryDailyReportResult_SummaryStudentUA>();

            foreach (var data in getDataEnrollmentAndAttandanceBySessionName.Where(x => x.AttendanceCode == "UA").OrderBy(x => x.GradeCode).ThenBy(x => x.HomeroomCode).ThenBy(x => x.StudentName).ToList())
            {
                var dataStudentAttandance = new GetAttendanceSummaryDailyReportResult_SummaryStudentUA
                {
                    Student = new NameValueVm
                    {
                        Id = data.IdStudent,
                        Name = data.StudentName,
                    },
                    Homeroom = new ItemValueVm
                    {
                        Id = data.GradeCode,
                        Description = data.HomeroomCode
                    },
                    ClassID = data.ClassID,
                    Session = data.SessionName,
                    GradeLevel = data.GradeCode
                };

                retVal.Add(dataStudentAttandance);
            }

            return retVal.OrderBy(x => int.Parse(x.Homeroom.Id)).ThenBy(x => x.Homeroom.Description).ThenBy(x => int.Parse(x.Session)).ThenBy(x => x.Student.Name).ToList();
        }

        public List<GetAttendanceSummaryDailyReportResult_PercentPerSession> GetSummaryPercentPerSession(List<GetAttendanceSummaryDailyReportResult_SummaryStudentUA> GetSummaryDetail)
        {
            var GetSummaryDetailGrouping = GetSummaryDetail
                   .GroupBy(x => x.Session)
                   .Select(group => new
                   {
                       SessionID = group.Key,
                       TotalUA = group.Where(x => x.Session == group.Key).Count()//,
                       //Percentage = ((group.Where(x => x.Session == group.Key).Count() / GetSummaryDetail.Count()) * 100).ToString()
                   }).OrderBy(x => x.SessionID)
                   .ToList();

            var retVal = new List<GetAttendanceSummaryDailyReportResult_PercentPerSession>();

            foreach (var item in GetSummaryDetailGrouping)
            {
                var totalUA = (decimal) item.TotalUA;
                var totalSummary = (decimal) GetSummaryDetail.Count();
                var Percentage = (decimal) (totalUA / totalSummary) * 100;

                var summerySeession = new GetAttendanceSummaryDailyReportResult_PercentPerSession
                {
                    SessionID = item.SessionID,
                    TotalUA = item.TotalUA,
                    Percentage = decimal.Round(Percentage, 2, MidpointRounding.AwayFromZero).ToString()
                };

                retVal.Add(summerySeession);
            }
           
            return retVal.OrderBy(x => int.Parse(x.SessionID)).ToList();
        }

        public List<GetAttendanceSummaryDailyReportResult_SummaryPerGradeSession> GetSummaryPerGradeSession(List<GetAttendanceSummaryDailyReportResult_SummaryStudentUA> GetSummaryDetail)
        {
            var retVal = GetSummaryDetail
                   .GroupBy(x => x.GradeLevel)
                   .Select(group => new GetAttendanceSummaryDailyReportResult_SummaryPerGradeSession
                   {
                       GradeLevel = group.Key,
                       TotalSession = group.Where(x => x.GradeLevel == group.Key).Count(),
                   }).ToList();

            return retVal.OrderBy(x => int.Parse(x.GradeLevel)).ToList();
        }

        public async Task<List<GetAttendanceSummaryDailyReportResult_PositionUser>> GetTeacherPosition(GetAttendanceSummaryDailyReportRequest param)
        {
            var positionUser = await _dbContext.Entity<TrNonTeachingLoad>()
                    .Include(x => x.NonTeachingLoad).ThenInclude(x => x.TeacherPosition).ThenInclude(x => x.LtPosition)
                    .Where(x => x.IdUser == param.IdUser &&
                                x.NonTeachingLoad.IdAcademicYear == param.IdAcademicYear)
                    .Select(x => new
                    {
                        x.Data,
                        x.NonTeachingLoad.TeacherPosition.LtPosition.Code
                    }).Distinct()
                    .ToListAsync(CancellationToken);

            var retVal = positionUser
                    .Select(x => new GetAttendanceSummaryDailyReportResult_PositionUser
                    {
                        Code = x.Code,
                        Data = x.Data
                    }).ToList();

            return retVal;
        }

        public async Task<List<string>> GetUserAvaiablePosition(GetAttendanceSummaryDailyReportRequest param, List<GetAttendanceSummaryDailyReportResult_PositionUser> getTeacherPosition)
        {
            List<string> avaiablePosition = new List<string>();

            var getHomeroomTeacherList = await _dbContext.Entity<MsHomeroomTeacher>()
                    .Where(x => x.IdBinusian == param.IdUser &&
                                x.Homeroom.IdAcademicYear == param.IdAcademicYear)
                    .Select(x => x.Homeroom.Id)
                    .Distinct()
                    .ToListAsync(CancellationToken);

            var getLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                    .Where(x => x.IdUser == param.IdUser &&
                                x.Lesson.IdAcademicYear == param.IdAcademicYear)
                    .Select(x => x.IdLesson)
                    .Distinct()
                    .ToListAsync(CancellationToken);

            if (getHomeroomTeacherList.Count > 0)
                avaiablePosition.Add(PositionConstant.ClassAdvisor);

            if (getLessonTeacher.Count > 0)
                avaiablePosition.Add(PositionConstant.SubjectTeacher);

            foreach (var pu in getTeacherPosition)
            {
                avaiablePosition.Add(pu.Code);
            }

            return avaiablePosition;
        }

        //public async Task<List<GetAttendanceSummaryDailyReportResult_SummaryPerGradeSession>> GetUserPrivileges(GetAttendanceSummaryDailyReportRequest param)
        //{
        //    #region Get Position User
        //    var positionUser = await GetTeacherPosition(param);
        //    var getUserAvaiablePosition = await GetUserAvaiablePosition(param, positionUser);

        //    var predicateLevel = PredicateBuilder.Create<MsLevel>(x => 1 == 1);
        //    var predicateLevelPrincipalAndVicePrincipal = PredicateBuilder.Create<MsHomeroom>(x => 1 == 1);
        //    var predicateHomeroom = PredicateBuilder.Create<MsHomeroom>(x => 1 == 1);
        //    var predicateLesson = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => 1 == 1);
        //    var predicateStudentGrade = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => 1 == 1);

        //    List<string> idLevelPrincipalAndVicePrincipal = new List<string>();

        //    foreach (var itemPosition in getUserAvaiablePosition)
        //    {
        //        if (itemPosition == PositionConstant.Principal)
        //        {
        //            var Principal = positionUser.Where(x => x.Code == PositionConstant.Principal).ToList();

        //            foreach (var item in Principal)
        //            {
        //                var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                _dataNewLH.TryGetValue("Level", out var _levelLH);
        //                idLevelPrincipalAndVicePrincipal.Add(_levelLH.Id);
        //            }
        //            predicateLevelPrincipalAndVicePrincipal = predicateLevelPrincipalAndVicePrincipal.And(x => idLevelPrincipalAndVicePrincipal.Contains(x.Grade.IdLevel));
        //        }
        //        if (itemPosition == PositionConstant.VicePrincipal)
        //        {
        //            var Principal = positionUser.Where(x => x.Code == PositionConstant.VicePrincipal).ToList();
        //            List<string> IdLevels = new List<string>();

        //            foreach (var item in Principal)
        //            {
        //                var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                _dataNewLH.TryGetValue("Level", out var _levelLH);
        //                IdLevels.Add(_levelLH.Id);
        //            }
        //            predicateLevelPrincipalAndVicePrincipal = predicateLevelPrincipalAndVicePrincipal.And(x => IdLevels.Contains(x.Grade.IdLevel));
        //        }
        //        if (itemPosition == PositionConstant.LevelHead)
        //        {
        //            var LevelHead = positionUser.Where(x => x.Code == PositionConstant.LevelHead).ToList();
        //            List<string> IdGrade = new List<string>();
        //            foreach (var item in LevelHead)
        //            {
        //                var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                _dataNewLH.TryGetValue("Level", out var _levelLH);
        //                _dataNewLH.TryGetValue("Grade", out var _gradeLH);
        //                IdGrade.Add(_gradeLH.Id);
        //            }
        //            predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
        //            predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
        //        }
        //        if (itemPosition == PositionConstant.SubjectHead)
        //        {
        //            var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHead).ToList();
        //            List<string> IdGrade = new List<string>();
        //            List<string> IdSubject = new List<string>();
        //            foreach (var item in LevelHead)
        //            {
        //                var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                _dataNewSH.TryGetValue("Level", out var _leveltSH);
        //                _dataNewSH.TryGetValue("Grade", out var _gradeSH);
        //                _dataNewSH.TryGetValue("Department", out var _departmentSH);
        //                _dataNewSH.TryGetValue("Subject", out var _subjectSH);
        //                IdGrade.Add(_gradeSH.Id);
        //                IdSubject.Add(_subjectSH.Id);
        //            }
        //            predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
        //            predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
        //            predicateLesson = predicateLesson.And(x => IdSubject.Contains(x.IdSubject));
        //        }
        //        if (itemPosition == PositionConstant.SubjectHeadAssitant)
        //        {
        //            var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHeadAssitant).ToList();
        //            List<string> IdGrade = new List<string>();
        //            List<string> IdSubject = new List<string>();
        //            foreach (var item in LevelHead)
        //            {
        //                var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                _dataNewSH.TryGetValue("Level", out var _leveltSH);
        //                _dataNewSH.TryGetValue("Grade", out var _gradeSH);
        //                _dataNewSH.TryGetValue("Department", out var _departmentSH);
        //                _dataNewSH.TryGetValue("Subject", out var _subjectSH);
        //                IdGrade.Add(_gradeSH.Id);
        //                IdSubject.Add(_subjectSH.Id);
        //            }
        //            predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
        //            predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
        //            predicateLesson = predicateLesson.And(x => IdSubject.Contains(x.IdSubject));
        //        }
        //        if (itemPosition == PositionConstant.HeadOfDepartment)
        //        {
        //            var HOD = positionUser.Where(x => x.Code == PositionConstant.HeadOfDepartment).ToList();
        //            List<string> idDepartment = new List<string>();
        //            List<string> IdGrade = new List<string>();
        //            List<string> IdSubject = new List<string>();

        //            foreach (var item in HOD)
        //            {
        //                var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
        //                _dataNewSH.TryGetValue("Department", out var _departmentSH);
        //                idDepartment.Add(_departmentSH.Id);
        //            }

        //            var departments = await _dbContext.Entity<MsDepartment>()
        //                .Include(x => x.DepartmentLevels)
        //                    .ThenInclude(x => x.Level)
        //                        .ThenInclude(x => x.Grades)
        //                .Where(x => idDepartment.Contains(x.Id))
        //                .Select(x => x)
        //                .ToListAsync(CancellationToken);
        //            var idDepartments = departments.Select(x => x.Id);

        //            var subjectByDepartments = await _dbContext.Entity<MsSubject>()
        //                .Include(x => x.Department)
        //                .Where(x => idDepartments.Contains(x.IdDepartment))
        //                .Select(x => new
        //                {
        //                    x.Id,
        //                    x.IdGrade,
        //                    x.Grade.IdLevel
        //                }
        //                )
        //                .ToListAsync(CancellationToken);

        //            foreach (var department in departments)
        //            {
        //                if (department.Type == DepartmentType.Level)
        //                {

        //                    foreach (var departmentLevel in department.DepartmentLevels)
        //                    {
        //                        var gradePerLevel = subjectByDepartments.Where(x => x.IdLevel == departmentLevel.IdLevel);
        //                        foreach (var grade in gradePerLevel)
        //                        {
        //                            IdGrade.Add(grade.IdGrade);
        //                            IdSubject.Add(grade.Id);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    foreach (var item in subjectByDepartments)
        //                    {
        //                        IdGrade.Add(item.IdGrade);
        //                        IdSubject.Add(item.Id);
        //                    }
        //                }
        //            }
        //            predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
        //            predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
        //            predicateLesson = predicateLesson.And(x => IdSubject.Contains(x.IdSubject));
        //        }
        //        if (itemPosition == PositionConstant.ClassAdvisor)
        //        {
        //            predicateHomeroom = PredicateBuilder.Create<MsHomeroom>(x => getHomeroomTeacherList.Contains(x.Id) && x.HomeroomTeachers.Any(ht => ht.IdBinusian == param.IdUser));
        //        }
        //        if (itemPosition == PositionConstant.SubjectTeacher)
        //        {
        //            predicateLesson = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => getLessonTeacher.Contains(x.IdLesson) && x.IdUser == param.IdUser);
        //            predicateHomeroom = predicateHomeroom.And(x => x.HomeroomPathways.Any(y => y.LessonPathways.Any(z => getLessonTeacher.Contains(z.IdLesson))));
        //        }
        //    }
        //    #endregion

        //    return retVal.OrderBy(x => int.Parse(x.GradeLevel)).ToList();
        //}
    }
}
