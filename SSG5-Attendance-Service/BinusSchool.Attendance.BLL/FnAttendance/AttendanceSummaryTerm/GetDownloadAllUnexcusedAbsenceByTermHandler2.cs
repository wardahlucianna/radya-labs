using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Utils;
using BinusSchool.Attendance.FnAttendance.Models;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using BinusSchool.Persistence.AttendanceDb.Models;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetDownloadAllUnexcusedAbsenceByTermHandler2 : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceSummaryRedisService _attendanceSummaryRedisService;
        private readonly IAttendanceSummaryService _attendanceSummaryService;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceSummary _apiAttendanceSummary;
        private readonly ILogger<GetDownloadAllUnexcusedAbsenceByTermHandler2> _logger;
        private readonly IMachineDateTime _datetime;

        public GetDownloadAllUnexcusedAbsenceByTermHandler2(
        IAttendanceSummaryRedisService attendanceSummaryRedisService,
        IAttendanceSummaryService attendanceSummaryService,
        IAttendanceDbContext dbContext,
        IAttendanceSummary apiAttendanceSummary,
        ILogger<GetDownloadAllUnexcusedAbsenceByTermHandler2> logger,
        IMachineDateTime dateTime)
        {
            _attendanceSummaryRedisService = attendanceSummaryRedisService;
            _attendanceSummaryService = attendanceSummaryService;
            _dbContext = dbContext;
            _apiAttendanceSummary = apiAttendanceSummary;
            _logger = logger;
            _datetime = dateTime;
        }
        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<GetDownloadAllUnexcusedAbsenceByTermRequest>();
            // ms mapping attendance
            var mappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                .Where(e => e.Level.IdAcademicYear == param.IdAcademicYear && e.IdLevel == param.IdLevel)
                .Select(e => new
                {
                    e.Id,
                    e.IdLevel,
                    e.AbsentTerms,
                    e.IsNeedValidation,
                    e.IsUseWorkhabit,
                    e.IsUseDueToLateness,
                })
                .FirstOrDefaultAsync(CancellationToken) ?? throw new Exception("Data mapping is missing");

            var listMappingSemesterGradeIdClassroomAndIdHomeroom =
                new List<(
                    int Semester,
                    string IdGrade,
                    string IdClassroom,
                    string ClassroomCode,
                    string IdHomeroom,
                    string GradeCode)>();
            var idGrades = new List<string>();
            if (string.IsNullOrWhiteSpace(param.IdGrade))
                idGrades.AddRange(await _dbContext.Entity<MsGrade>()
                    .AsNoTracking()
                    .Where(e => e.Level.Id == param.IdLevel)
                    .Select(e => e.Id)
                    .ToListAsync(CancellationToken));
            else
                idGrades.Add(param.IdGrade);

            var homeroomQueryable = _dbContext.Entity<MsHomeroom>()
                .AsNoTracking()
                .Where(e => idGrades.Contains(e.IdGrade) && e.Grade.Level.Id == param.IdLevel)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(param.IdClassroom))
                homeroomQueryable = homeroomQueryable.Where(x => x.GradePathwayClassroom.Classroom.Id == param.IdClassroom);

            var homerooms = await homeroomQueryable
                .Select(e => new
                {
                    IdHomeroom = e.Id,
                    IdClassroom = e.GradePathwayClassroom.Classroom.Id,
                    ClassroomCode = e.GradePathwayClassroom.Classroom.Code,
                    ClassroomDesc = e.GradePathwayClassroom.Classroom.Description,
                    e.Semester,
                    e.IdGrade,
                    GradeCode = e.Grade.Code
                })
                .ToListAsync(CancellationToken);

            if (!homerooms.Any())
                throw new Exception("Invalid homeroom");

            foreach (var item in homerooms)
                listMappingSemesterGradeIdClassroomAndIdHomeroom.Add((
                    item.Semester,
                    item.IdGrade,
                    item.IdClassroom,
                    item.ClassroomCode,
                    item.IdHomeroom,
                    item.GradeCode));

            var dict = new Dictionary<(int Semester, string IdGrade, string IdHomeroom), List<StudentEnrollmentDto2>>();
            foreach (var item in listMappingSemesterGradeIdClassroomAndIdHomeroom)
                dict.Add((item.Semester, item.IdGrade, item.IdHomeroom),
                    await _attendanceSummaryService.GetStudentEnrolledAsync(
                        item.IdHomeroom,
                        DateTime.MinValue, CancellationToken));

            var listIdStudent = dict.SelectMany(e => e.Value).Select(e => e.IdStudent).Distinct().ToArray();
            var studentStatuses =
                await _attendanceSummaryService.GetStudentStatusesAsync(listIdStudent, param.IdAcademicYear,
                    CancellationToken);

            var starDate = DateTime.Parse("1999-01-01");
            var endDate = DateTime.Parse("1999-01-01");
            if (!string.IsNullOrEmpty(param.IdPeriod))
            {
                var period = await _dbContext.Entity<MsPeriod>()
                                .Where(x => x.IdGrade == param.IdGrade && x.Id.Contains(param.IdPeriod))
                                .Select(e => new
                                {
                                    e.StartDate,
                                    e.EndDate
                                })
                                .FirstOrDefaultAsync();
                if (period != null)
                {
                    starDate = period.StartDate;
                    endDate = period.EndDate;
                }
            }
            else
            {
                if (param.Semester != null)
                {
                    var period = await _dbContext.Entity<MsPeriod>()
                        .Where(x => x.IdGrade == param.IdGrade && x.Semester == param.Semester)
                        .Select(e => new
                        {
                            e.StartDate,
                            e.EndDate
                        })
                        .ToListAsync();

                    if (period.Any())
                    {
                        starDate = period.Select(x => x.StartDate).Min();
                        endDate = period.Select(x => x.EndDate).Max();
                    }
                    else
                    {
                        throw new Exception("Period Not Found");
                    }
                }
            }

            // Get Entry Student
            var getAttendanceEntries = await _attendanceSummaryService.GetAttendanceEntryUnexcusedAbsenceAsync(param.IdAcademicYear, param.IdLevel,
                param.IdGrade, studentStatuses.Select(x => x.IdStudent).ToList(), starDate, endDate, CancellationToken);
            if (!getAttendanceEntries.Any())
                throw new Exception("No Data Entries");

            List<GetDownloadAllUnexcusedAbsenceByPeriodResult> listAllUnexcusedAbsence = new List<GetDownloadAllUnexcusedAbsenceByPeriodResult>();

            //get lesson teacher from lesson attendance entry
            var listLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                     .Include(e => e.Lesson)
                                     .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear
                                             && e.IsAttendance
                                             && getAttendanceEntries.Select(x => x.IdLesson).Distinct().ToList().Contains(e.IdLesson)
                                             && e.IsPrimary)
                                     .ToListAsync(CancellationToken);

            //set user entry from attendance administration from lesson teacher
            foreach (var data in getAttendanceEntries.Where(x => x.IsFromAttendanceAdministration).ToList())
            {
                data.IdUserAttendanceEntry = listLessonTeacher.Where(x => x.IdLesson == data.IdLesson).Select(x => x.IdUser).FirstOrDefault();
            }

            var listIdUserTeacher = getAttendanceEntries.Select(e => e.IdUserAttendanceEntry).Distinct().ToList();

            var listUser = await _dbContext.Entity<MsUser>()
                        .Where(e => listIdUserTeacher.Contains(e.Id))
                        .Select(e => new
                        {
                            Id = e.Id,
                            DisplayName = e.DisplayName
                        })
                        .ToListAsync(CancellationToken);

            foreach (var itemAttendance in getAttendanceEntries)
            {
                //filter student status
                if (!studentStatuses.Any(e => e.StartDt.Date <= itemAttendance.ScheduleDate.Date && e.EndDt >= itemAttendance.ScheduleDate.Date && e.IdStudent == itemAttendance.IdStudent))
                    continue;

                var teacherName = listUser.Where(e => e.Id == itemAttendance.IdUserAttendanceEntry).Select(e => e.DisplayName).FirstOrDefault();

                var getStudentEnrollment = dict[(itemAttendance.Semester, itemAttendance.IdGrade, itemAttendance.IdHomeroom)];

                var studentEnrolled = getStudentEnrollment.Where(e => e.IdHomeroomStudent == itemAttendance.IdHomeroomStudent).FirstOrDefault();

                var passed = false;
                //logic moving
                foreach (var current in studentEnrolled.Items)
                {
                    if (passed)
                        break;

                    if (string.IsNullOrWhiteSpace(current.IdLesson))
                        continue;

                    if (current.Ignored || current.IdLesson != itemAttendance.IdLesson)
                        continue;

                    //when current id lesson is same as above and the date of the current moving still satisfied
                    //then set to passed, other than that will be excluded
                    if (current.StartDt.Date <= itemAttendance.ScheduleDate.Date &&
                        itemAttendance.ScheduleDate.Date < current.EndDt.Date)
                        passed = true;
                }

                if (!passed)
                    continue;

                var AllUnexcusedAbsence = new GetDownloadAllUnexcusedAbsenceByPeriodResult
                {
                    StudentId = itemAttendance.Student.IdStudent,
                    StudentName = NameUtil.GenerateFullName(itemAttendance.Student.FirstName, itemAttendance.Student.MiddleName, itemAttendance.Student.LastName),
                    Class = itemAttendance.GradeCode + itemAttendance.Classroom.Code,
                    DateOfAbsence = itemAttendance.ScheduleDate,
                    ClassSession = itemAttendance.Session.SessionID,
                    Subject = itemAttendance.Subject.Description,
                    TeacherName = teacherName,
                    AttendanceStatus = itemAttendance.Attendance.Description,
                };

                listAllUnexcusedAbsence.Add(AllUnexcusedAbsence);
            }

            var isDailyAttendance = false;
            if (mappingAttendance.AbsentTerms == AbsentTerm.Day)
                isDailyAttendance = true;

            var excelRecap = GenerateExcel(listAllUnexcusedAbsence, isDailyAttendance);

            return new FileContentResult(excelRecap, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Recap-All-Unexcused-Absence{DateTime.Now.Ticks}.xlsx"
            };
        }

        private byte[] GenerateExcel(List<GetDownloadAllUnexcusedAbsenceByPeriodResult> data, bool isDailyAttendance)
        {
            var param = Request.ValidateParams<GetDownloadAllUnexcusedAbsenceByRangeRequest>();

            var workbook = new XSSFWorkbook();

            var fontBold = workbook.CreateFont();
            fontBold.IsBold = true;

            var boldStyle = workbook.CreateCellStyle();
            boldStyle.SetFont(fontBold);

            var borderCellStyle = workbook.CreateCellStyle();
            borderCellStyle.BorderBottom = BorderStyle.Thin;
            borderCellStyle.BorderLeft = BorderStyle.Thin;
            borderCellStyle.BorderRight = BorderStyle.Thin;
            borderCellStyle.BorderTop = BorderStyle.Thin;

            var headerCellStyle = workbook.CreateCellStyle();
            headerCellStyle.CloneStyleFrom(borderCellStyle);
            headerCellStyle.SetFont(fontBold);

            var sheet = workbook.CreateSheet();
            var rowHeader = sheet.CreateRow(0);

            if (isDailyAttendance == true)
            {
                var cellNo = rowHeader.CreateCell(0);
                cellNo.SetCellValue("No");
                cellNo.CellStyle = boldStyle;
                var cellStudentId = rowHeader.CreateCell(1);
                cellStudentId.SetCellValue("Student ID");
                cellStudentId.CellStyle = boldStyle;
                var cellStudentName = rowHeader.CreateCell(2);
                cellStudentName.SetCellValue("Student Name");
                cellStudentName.CellStyle = boldStyle;
                var cellClass = rowHeader.CreateCell(3);
                cellClass.SetCellValue("Class");
                cellClass.CellStyle = boldStyle;
                var cellDate = rowHeader.CreateCell(4);
                cellDate.SetCellValue("Date of Absence");
                cellDate.CellStyle = boldStyle;
                var cellTeacher = rowHeader.CreateCell(5);
                cellTeacher.SetCellValue("Teacher's Name");
                cellTeacher.CellStyle = boldStyle;
                var cellAttendanceStatus = rowHeader.CreateCell(6);
                cellAttendanceStatus.SetCellValue("Attendance Status");
                cellAttendanceStatus.CellStyle = boldStyle;

                int rowIndex = 1;
                int startColumn = 0;

                var TeacherName = _dbContext.Entity<MsHomeroomTeacher>()
                                        .Include(e => e.Staff)
                                        .Where(x => x.Homeroom.IdGrade == param.IdGrade && x.IsAttendance)
                                        .Select(x => (x.Staff.FirstName == null ? "" : x.Staff.FirstName) + (x.Staff.FirstName == null ? "" : " " + x.Staff.LastName)).FirstOrDefault();

                var listDataGrouping = data.GroupBy(e => new
                {
                    e.StudentId,
                    e.StudentName,
                    e.Class,
                    e.DateOfAbsence,
                    e.AttendanceStatus
                }).Select(e => e.Key).ToList();

                foreach (var itemData in listDataGrouping)
                {
                    rowHeader = sheet.CreateRow(rowIndex);
                    cellNo = rowHeader.CreateCell(0);
                    cellNo.SetCellValue(rowIndex);
                    cellStudentId = rowHeader.CreateCell(1);
                    cellStudentId.SetCellValue(itemData.StudentId);
                    cellStudentName = rowHeader.CreateCell(2);
                    cellStudentName.SetCellValue(itemData.StudentName);
                    cellClass = rowHeader.CreateCell(3);
                    cellClass.SetCellValue(itemData.Class);
                    cellDate = rowHeader.CreateCell(4);
                    cellDate.SetCellValue(itemData.DateOfAbsence.ToString("dd MMMM yyyy"));
                    cellTeacher = rowHeader.CreateCell(5);
                    cellTeacher.SetCellValue(TeacherName);
                    cellAttendanceStatus = rowHeader.CreateCell(6);
                    cellAttendanceStatus.SetCellValue(itemData.AttendanceStatus);

                    rowIndex++;
                    startColumn++;
                }

                using var ms = new MemoryStream();
                //ms.Position = 0;
                workbook.Write(ms);

                return ms.ToArray();
            }
            else
            {
                var cellNo = rowHeader.CreateCell(0);
                cellNo.SetCellValue("No");
                cellNo.CellStyle = boldStyle;
                var cellStudentId = rowHeader.CreateCell(1);
                cellStudentId.SetCellValue("Student ID");
                cellStudentId.CellStyle = boldStyle;
                var cellStudentName = rowHeader.CreateCell(2);
                cellStudentName.SetCellValue("Student Name");
                cellStudentName.CellStyle = boldStyle;
                var cellClass = rowHeader.CreateCell(3);
                cellClass.SetCellValue("Class");
                cellClass.CellStyle = boldStyle;
                var cellDate = rowHeader.CreateCell(4);
                cellDate.SetCellValue("Date of Absence");
                cellDate.CellStyle = boldStyle;
                var cellSession = rowHeader.CreateCell(5);
                cellSession.SetCellValue("Session");
                cellSession.CellStyle = boldStyle;
                var cellSubject = rowHeader.CreateCell(6);
                cellSubject.SetCellValue("Subject");
                cellSubject.CellStyle = boldStyle;
                var cellTeacher = rowHeader.CreateCell(7);
                cellTeacher.SetCellValue("Teacher's Name");
                cellTeacher.CellStyle = boldStyle;
                var cellAttendanceStatus = rowHeader.CreateCell(8);
                cellAttendanceStatus.SetCellValue("Attendance Status");
                cellAttendanceStatus.CellStyle = boldStyle;

                int rowIndex = 1;
                int startColumn = 0;

                foreach (var itemData in data)
                {
                    rowHeader = sheet.CreateRow(rowIndex);
                    cellNo = rowHeader.CreateCell(0);
                    cellNo.SetCellValue(rowIndex);
                    cellStudentId = rowHeader.CreateCell(1);
                    cellStudentId.SetCellValue(itemData.StudentId);
                    cellStudentName = rowHeader.CreateCell(2);
                    cellStudentName.SetCellValue(itemData.StudentName);
                    cellClass = rowHeader.CreateCell(3);
                    cellClass.SetCellValue(itemData.Class);
                    cellDate = rowHeader.CreateCell(4);
                    cellDate.SetCellValue(itemData.DateOfAbsence.ToString("dd MMMM yyyy"));
                    cellSession = rowHeader.CreateCell(5);
                    cellSession.SetCellValue(itemData.ClassSession);
                    cellSubject = rowHeader.CreateCell(6);
                    cellSubject.SetCellValue(itemData.Subject);
                    cellTeacher = rowHeader.CreateCell(7);
                    cellTeacher.SetCellValue(itemData.TeacherName);
                    cellAttendanceStatus = rowHeader.CreateCell(8);
                    cellAttendanceStatus.SetCellValue(itemData.AttendanceStatus);

                    rowIndex++;
                    startColumn++;
                }

                using var ms = new MemoryStream();
                //ms.Position = 0;
                workbook.Write(ms);

                return ms.ToArray();
            }

        }
    }
}
