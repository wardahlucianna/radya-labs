using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetDownloadAllUnexcusedAbsenceByTermHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        private readonly IRedisCache _redisCache;

        public GetDownloadAllUnexcusedAbsenceByTermHandler(IAttendanceDbContext AttendanceDbContext, IMachineDateTime datetime, IRedisCache redisCache)
        {
            _dbContext = AttendanceDbContext;
            _datetime = datetime;
            _redisCache = redisCache;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            FillConfiguration();

            var param = Request.ValidateParams<GetDownloadAllUnexcusedAbsenceByTermRequest>();
            var filterIdHomerooms = new GetHomeroomByIdUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                SelectedPosition = param.SelectedPosition,
                IdUser = param.IdUser,
                IdClassroom=param.IdClassroom,
                IdGrade = param.IdGrade,
                IdLevel=param.IdLevel,
                Semester=param.Semester
            };

            var IdHomerooms = await GetAttendanceSummaryHandler.GetHomeroomByUser(_dbContext, CancellationToken, filterIdHomerooms);

            #region GetRedis
            var paramRedis = new RedisAttendanceSummaryRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel
            };

            var redisStudentStatus = await AttendanceSummaryRedisCacheHandler.GetStudentStatus(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime);
            var redisHomeroomStudentEnrollment = await AttendanceSummaryRedisCacheHandler.GetHomeroomStudentEnrollment(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisTrHomeroomStudentEnrollment = await AttendanceSummaryRedisCacheHandler.GetTrHomeroomStudentEnrollment(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisAttendanceEntry = await AttendanceSummaryRedisCacheHandler.GetAttendanceEntry(paramRedis, _redisCache, _dbContext, CancellationToken, _datetime.ServerTime);
            var redisPeriod = await AttendanceSummaryRedisCacheHandler.GetPeriod(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisSchedule = await AttendanceSummaryRedisCacheHandler.GetSchedule(paramRedis, _redisCache, _dbContext, CancellationToken);
            var redisMappingAttendance = await AttendanceSummaryRedisCacheHandler.GetMappingAttendance(paramRedis, _redisCache, _dbContext, CancellationToken);
            #endregion

            var listIdStudentActive = redisStudentStatus
                                       .Where(x => x.StartDate.Date <= _datetime.ServerTime.Date
                                              && x.EndDate.Date >= _datetime.ServerTime.Date)
                                       .Select(e => e.IdStudent)
                                       .ToList();

            var queryAttendance = redisAttendanceEntry
                            .Where(x => x.Attendance.AbsenceCategory == AbsenceCategory.Unexcused && x.Status== AttendanceEntryStatus.Submitted);

            if (!string.IsNullOrEmpty(param.IdGrade))
                queryAttendance = queryAttendance.Where(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdPeriod))
            {
                    var period = redisPeriod
                                    .Where(x =>x.IdGrade == param.IdGrade && x.Id.Contains(param.IdPeriod))
                                    .Select(e => new
                                    {
                                        e.StartDate,
                                        e.EndDate
                                    })
                                    .FirstOrDefault();

                queryAttendance = queryAttendance.Where(x => x.ScheduleDate.Date >= period.StartDate.Date && x.ScheduleDate.Date <= period.EndDate.Date);

            }

            var attendance = queryAttendance.ToList();

            List<GetDownloadAllUnexcusedAbsenceByPeriodResult> listAllUnexcusedAbsence = new List<GetDownloadAllUnexcusedAbsenceByPeriodResult>();

            foreach (var itemAttendance in attendance)
            {
                var listStatusStudentByDate = redisStudentStatus.Where(e => e.StartDate.Date <= itemAttendance.ScheduleDate.Date).Select(e => e.IdStudent).ToList();

                //moving student
                var listStudentEnrolmentBySchedule = redisHomeroomStudentEnrollment
                                    .Where(e => e.IdHomeroomStudent == itemAttendance.IdHomeroomStudent && e.Semester == itemAttendance.Semester)
                                    .ToList();

                var listTrStudentEnrolmentBySchedule = redisTrHomeroomStudentEnrollment
                                            .Where(e => e.IdHomeroomStudent == itemAttendance.IdHomeroomStudent && e.Semester == itemAttendance.Semester)
                                            .ToList();

                var listStudentEnrollmentUnion = listStudentEnrolmentBySchedule.Union(listTrStudentEnrolmentBySchedule)
                                                   .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                   .ToList();

                var listStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, itemAttendance.ScheduleDate, itemAttendance.Semester.ToString(), itemAttendance.IdLesson);

                var studentEnrollmentMoving = listStudentEnrollmentMoving
                                              .Where(e => listStatusStudentByDate.Contains(e.IdStudent))
                                              .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                              .ToList();

                if (studentEnrollmentMoving.Any())
                {
                    var TeacherData = redisSchedule.Where(e => e.IdLesson == itemAttendance.IdLesson
                                            && e.IdSession == itemAttendance.Session.Id
                                            && e.IdWeek == itemAttendance.IdWeek
                                            && e.IdDay == itemAttendance.IdDay)
                                        .Select(x => new {
                                                x.Teacher.FirstName,
                                                x.Teacher.LastName
                                            }).Distinct().ToList();

                    var Teacher = string.Empty;
                    foreach (var item in TeacherData)
                    {
                        Teacher = NameUtil.GenerateFullName(item.FirstName,item.LastName);
                    }

                   

                    var AllUnexcusedAbsence = new GetDownloadAllUnexcusedAbsenceByPeriodResult
                    {
                        StudentId = itemAttendance.Student.IdStudent,
                        StudentName = NameUtil.GenerateFullName(itemAttendance.Student.FirstName, itemAttendance.Student.MiddleName, itemAttendance.Student.LastName),
                        Class = itemAttendance.GradeCode + itemAttendance.Classroom.Code,
                        DateOfAbsence = itemAttendance.ScheduleDate,
                        ClassSession = itemAttendance.Session.SessionID,
                        Subject = itemAttendance.Subject.Description,
                        TeacherName = Teacher,
                        AttendanceStatus = itemAttendance.Attendance.Description,
                    };

                    listAllUnexcusedAbsence.Add(AllUnexcusedAbsence);
                }
            }

            var isDailyAttendance = false;
            if (redisMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Day).Any())
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
                                        .Include(e=>e.Staff)
                                        .Where(x => x.Homeroom.IdGrade == param.IdGrade && x.IsAttendance)
                                        .Select(x => (x.Staff.FirstName==null?"":x.Staff.FirstName) + (x.Staff.FirstName == null ? "" : " " + x.Staff.LastName)).FirstOrDefault();

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
