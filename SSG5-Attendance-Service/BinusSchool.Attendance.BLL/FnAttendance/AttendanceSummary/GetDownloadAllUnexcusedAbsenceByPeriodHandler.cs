using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDownloadAllUnexcusedAbsenceByPeriodHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetListTeacherByInvitationRequest.IdInvitationBookingSetting),
        };
        private static readonly string[] _columns = { "StudentName", "TeacherName" };
        private readonly IAttendanceDbContext _dbContext;

        public GetDownloadAllUnexcusedAbsenceByPeriodHandler(IAttendanceDbContext AttendanceDbContext)
        {
            _dbContext = AttendanceDbContext;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            FillConfiguration();

            var param = Request.ValidateParams<GetDownloadAllUnexcusedAbsenceByPeriodRequest>(nameof(GetDownloadAllUnexcusedAbsenceByPeriodRequest.IdAcademicYear),
                                                                               nameof(GetDownloadAllUnexcusedAbsenceByPeriodRequest.IdLevel),
                                                                               nameof(GetDownloadAllUnexcusedAbsenceByPeriodRequest.Semester));
            #region Get Position User
            List<string> avaiablePosition = new List<string>();
            var dataHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
            .Include(x => x.Homeroom)
            .Where(x => x.IdBinusian == param.IdUser)
            .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear)
            .Distinct()
            .Select(x => x.Homeroom.Id).ToListAsync(CancellationToken);

            var dataLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
            .Include(x => x.Lesson)
            .Where(x => x.IdUser == param.IdUser)
            .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear)
            .Distinct()
            .Select(x => x.IdLesson).ToListAsync(CancellationToken);
            var positionUser = await _dbContext.Entity<TrNonTeachingLoad>().Include(x => x.NonTeachingLoad).ThenInclude(x => x.TeacherPosition).ThenInclude(x => x.LtPosition)
                .Where(x => x.NonTeachingLoad.IdAcademicYear == param.IdAcademicYear)
                .Where(x => x.IdUser == param.IdUser)
                .Select(x => new
                {
                    x.Data,
                    x.NonTeachingLoad.TeacherPosition.LtPosition.Code
                }).ToListAsync(CancellationToken);
            // if (positionUser.Count == 0 && dataHomeroomTeacher.Count == 0 && dataLessonTeacher.Count == 0)
            //     throw new BadRequestException($"You dont have any position.");
            if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
                avaiablePosition.Add(PositionConstant.ClassAdvisor);
            if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
                avaiablePosition.Add(PositionConstant.SubjectTeacher);
            foreach (var pu in positionUser)
            {
                avaiablePosition.Add(pu.Code);
            }
            // if (avaiablePosition.Where(x => x == param.SelectedPosition).Count() == 0)
            //     throw new BadRequestException($"You dont assign as {param.SelectedPosition}.");
            var predicateLevel = PredicateBuilder.Create<MsLevel>(x => 1 == 1);
            var predicateLevelPrincipalAndVicePrincipal = PredicateBuilder.Create<MsHomeroom>(x => 1 == 1);
            var predicateHomeroom = PredicateBuilder.Create<MsHomeroom>(x => 1 == 1);
            var predicateLesson = PredicateBuilder.Create<TrAttendanceEntry>(x => 1 == 1);
            var predicateStudentGrade = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => 1 == 1);
            List<string> idLevelPrincipalAndVicePrincipal = new List<string>();
            if (positionUser.Count > 0)
            {
                if (param.SelectedPosition == PositionConstant.Principal)
                {
                    if (positionUser.Any(y => y.Code == PositionConstant.Principal)) //check P Or VP
                    {
                        if (positionUser.Where(y => y.Code == PositionConstant.Principal).ToList() != null && positionUser.Where(y => y.Code == PositionConstant.Principal).Count() > 0)
                        {
                            var Principal = positionUser.Where(x => x.Code == PositionConstant.Principal).ToList();

                            foreach (var item in Principal)
                            {
                                var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                                _dataNewLH.TryGetValue("Level", out var _levelLH);
                                idLevelPrincipalAndVicePrincipal.Add(_levelLH.Id);
                            }
                            predicateLevelPrincipalAndVicePrincipal = predicateLevelPrincipalAndVicePrincipal.And(x => idLevelPrincipalAndVicePrincipal.Contains(x.Grade.IdLevel));
                        }
                    }
                }
                if (param.SelectedPosition == PositionConstant.VicePrincipal)
                {
                    if (positionUser.Any(y => y.Code == PositionConstant.VicePrincipal))
                    {
                        if (positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).ToList() != null && positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).Count() > 0)
                        {
                            var Principal = positionUser.Where(x => x.Code == PositionConstant.VicePrincipal).ToList();
                            List<string> IdLevels = new List<string>();
                            foreach (var item in Principal)
                            {
                                var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                                _dataNewLH.TryGetValue("Level", out var _levelLH);
                                IdLevels.Add(_levelLH.Id);
                            }
                            predicateLevelPrincipalAndVicePrincipal = predicateLevelPrincipalAndVicePrincipal.And(x => IdLevels.Contains(x.Grade.IdLevel));
                        }
                    }
                }
                if (param.SelectedPosition == PositionConstant.LevelHead)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.LevelHead).ToList() != null)
                    {
                        var LevelHead = positionUser.Where(x => x.Code == PositionConstant.LevelHead).ToList();
                        List<string> IdGrade = new List<string>();
                        foreach (var item in LevelHead)
                        {
                            var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewLH.TryGetValue("Level", out var _levelLH);
                            _dataNewLH.TryGetValue("Grade", out var _gradeLH);
                            IdGrade.Add(_gradeLH.Id);
                        }
                        predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
                        //predicateStudentGrade = predicateStudentGrade.And(x => x.TrGeneratedScheduleStudent.MsStudent.StudentGrades.Any(g => IdGrade.Contains(g.IdGrade)));
                    }
                }
                if (param.SelectedPosition == PositionConstant.SubjectHead)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.SubjectHead).ToList() != null)
                    {
                        var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHead).ToList();
                        List<string> IdGrade = new List<string>();
                        List<string> IdSubject = new List<string>();
                        foreach (var item in LevelHead)
                        {
                            var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewSH.TryGetValue("Level", out var _leveltSH);
                            _dataNewSH.TryGetValue("Grade", out var _gradeSH);
                            _dataNewSH.TryGetValue("Department", out var _departmentSH);
                            _dataNewSH.TryGetValue("Subject", out var _subjectSH);
                            IdGrade.Add(_gradeSH.Id);
                            IdSubject.Add(_subjectSH.Id);
                        }
                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
                        predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
                        predicateLesson = predicateLesson.And(x => IdSubject.Contains(x.GeneratedScheduleLesson.IdSubject));
                        //predicateStudentGrade = predicateStudentGrade.And(x => x.TrGeneratedScheduleStudent.MsStudent.StudentGrades.Any(g => IdGrade.Contains(g.IdGrade)));
                    }
                }
                if (param.SelectedPosition == PositionConstant.SubjectHeadAssitant)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.SubjectHeadAssitant).ToList() != null)
                    {
                        var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHeadAssitant).ToList();
                        List<string> IdGrade = new List<string>();
                        List<string> IdSubject = new List<string>();
                        foreach (var item in LevelHead)
                        {
                            var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewSH.TryGetValue("Level", out var _leveltSH);
                            _dataNewSH.TryGetValue("Grade", out var _gradeSH);
                            _dataNewSH.TryGetValue("Department", out var _departmentSH);
                            _dataNewSH.TryGetValue("Subject", out var _subjectSH);
                            IdGrade.Add(_gradeSH.Id);
                            IdSubject.Add(_subjectSH.Id);
                        }
                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
                        predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
                        predicateLesson = predicateLesson.And(x => IdSubject.Contains(x.GeneratedScheduleLesson.IdSubject));
                        //predicateStudentGrade = predicateStudentGrade.And(x => x.TrGeneratedScheduleStudent.MsStudent.StudentGrades.Any(g => IdGrade.Contains(g.IdGrade)));
                    }
                }
                if (param.SelectedPosition == PositionConstant.HeadOfDepartment)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.HeadOfDepartment).ToList() != null)
                    {
                        var HOD = positionUser.Where(x => x.Code == PositionConstant.HeadOfDepartment).ToList();
                        List<string> idDepartment = new List<string>();
                        List<string> IdGrade = new List<string>();
                        List<string> IdSubject = new List<string>();
                        foreach (var item in HOD)
                        {
                            var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewSH.TryGetValue("Department", out var _departmentSH);
                            idDepartment.Add(_departmentSH.Id);
                        }
                        var departments = await _dbContext.Entity<MsDepartment>()
                            .Include(x => x.DepartmentLevels)
                                .ThenInclude(x => x.Level)
                                    .ThenInclude(x => x.Grades)
                            .Where(x => idDepartment.Contains(x.Id))
                            .Select(x => x)
                            .ToListAsync(CancellationToken);
                        var idDepartments = departments.Select(x => x.Id);
                        var subjectByDepartments = await _dbContext.Entity<MsSubject>()
                            .Include(x => x.Department)
                            .Where(x => idDepartments.Contains(x.IdDepartment))
                            .Select(x => new
                            {
                                x.Id,
                                x.IdGrade,
                                x.Grade.IdLevel
                            }
                            )
                            .ToListAsync(CancellationToken);
                        //var IdAcademicYear = departments.Select(x => x.IdAcademicYear).Distinct().ToList();
                        //var gradeInAy = await _dbContext.Entity<MsGrade>()
                        //           .Include(x => x.MsLevel)
                        //           .Where(x => IdAcademicYear.Contains(x.MsLevel.IdAcademicYear))
                        //           .Select(x => new
                        //           {
                        //               idGrade = x.Id,
                        //               IdLevel = x.MsLevel.Id,
                        //               idAcademicYear = x.MsLevel.IdAcademicYear
                        //           })
                        //           .ToListAsync(CancellationToken);
                        foreach (var department in departments)
                        {
                            if (department.Type == DepartmentType.Level)
                            {

                                foreach (var departmentLevel in department.DepartmentLevels)
                                {
                                    var gradePerLevel = subjectByDepartments.Where(x => x.IdLevel == departmentLevel.IdLevel);
                                    foreach (var grade in gradePerLevel)
                                    {
                                        IdGrade.Add(grade.IdGrade);
                                        IdSubject.Add(grade.Id);
                                    }
                                }
                            }
                            else
                            {
                                //var gradeByAcademicYearDepartment = gradeInAy.Where(x => x.idAcademicYear == department.IdAcademicYear).ToList();
                                foreach (var item in subjectByDepartments)
                                {
                                    IdGrade.Add(item.IdGrade);
                                    IdSubject.Add(item.Id);
                                }
                            }
                        }
                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
                        predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
                        predicateLesson = predicateLesson.And(x => IdSubject.Contains(x.GeneratedScheduleLesson.IdSubject));
                        //predicateStudentGrade = predicateStudentGrade.And(x => x.TrGeneratedScheduleStudent.MsStudent.StudentGrades.Any(g => IdGrade.Contains(g.IdGrade)));
                    }
                }
            }
            else
            {
                if (param.SelectedPosition == PositionConstant.ClassAdvisor)
                {
                    if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
                        predicateHomeroom = PredicateBuilder.Create<MsHomeroom>(x => dataHomeroomTeacher.Contains(x.Id) && x.HomeroomTeachers.Any(ht => ht.IdBinusian == param.IdUser));
                }

                if (param.SelectedPosition == PositionConstant.SubjectTeacher)
                {
                    if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
                    {
                        predicateLesson = PredicateBuilder.Create<TrAttendanceEntry>(x => dataLessonTeacher.Contains(x.GeneratedScheduleLesson.IdLesson) && x.GeneratedScheduleLesson.IdUser == param.IdUser);
                        predicateHomeroom = predicateHomeroom.And(x => x.HomeroomPathways.Any(y => y.LessonPathways.Any(z => dataLessonTeacher.Contains(z.IdLesson))));
                    }

                }
            }
            #endregion

            if (!string.IsNullOrEmpty(param.ClassId))
                predicateLesson = predicateLesson.And(x => x.GeneratedScheduleLesson.ClassID == param.ClassId);
            if (!string.IsNullOrEmpty(param.IdSession))
                predicateLesson = predicateLesson.And(x => x.GeneratedScheduleLesson.IdSession == param.IdSession);
            var homerooms = await _dbContext.Entity<MsHomeroom>()
                                            .Include(x => x.HomeroomTeachers).ThenInclude(x => x.Staff)
                                            .Include(x => x.Grade).ThenInclude(x => x.Level)
                                            .Where(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear
                                                        && x.Grade.IdLevel == param.IdLevel
                                                        && x.Semester == param.Semester
                                                        && (string.IsNullOrEmpty(param.IdGrade) || x.IdGrade == param.IdGrade)
                                                        && (string.IsNullOrEmpty(param.IdHomeroom) || x.Id == param.IdHomeroom))
                                            .Where(predicateHomeroom)
                                            .ToListAsync(CancellationToken);
            var homeroomIds = homerooms.Select(x => x.Id).ToList();

            List<string> allowIsAttendanceEntryByClassId = new List<string>();
            if (param.SelectedPosition == PositionConstant.SubjectTeacher)
            {
                allowIsAttendanceEntryByClassId = await _dbContext.Entity<MsLessonTeacher>()
               .Include(x => x.Lesson)
               .Where(x => x.IdUser == param.IdUser)
               .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear)
               .Where(x => x.IsAttendance)
               .Select(x => x.Lesson.Id)
               .ToListAsync(CancellationToken);
                predicateLesson = predicateLesson.And(x => allowIsAttendanceEntryByClassId.Contains(x.GeneratedScheduleLesson.IdLesson));
            }
            if (param.SelectedPosition == PositionConstant.ClassAdvisor)
            {
                allowIsAttendanceEntryByClassId = await _dbContext.Entity<MsHomeroomTeacher>()
              .Include(x => x.Homeroom)
              .Where(x => x.IdBinusian == param.IdUser)
              .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear)
              .Where(x => x.IsAttendance)
              .Select(x => x.Homeroom.Id)
              .ToListAsync(CancellationToken);

                predicateLesson = predicateLesson.And(x => allowIsAttendanceEntryByClassId.Contains(x.GeneratedScheduleLesson.IdHomeroom));
            }

            var schedules = await _dbContext.Entity<TrAttendanceEntry>()
                            .Include(x => x.GeneratedScheduleLesson).ThenInclude(x => x.Homeroom).ThenInclude(x => x.Grade)
                            .Include(x => x.GeneratedScheduleLesson).ThenInclude(x => x.GeneratedScheduleStudent).ThenInclude(x => x.Student)
                            .Include(x => x.GeneratedScheduleLesson).ThenInclude(x => x.Session)
                            .Include(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                            .Where(x => x.AttendanceMappingAttendance.Attendance.Code == "UA")
                            .Where(x => x.GeneratedScheduleLesson.IsGenerated
                                        && homeroomIds.Contains(x.GeneratedScheduleLesson.IdHomeroom))
                            .Where(predicateLesson)
                            .ToListAsync(CancellationToken);

            var resultAttendance = schedules.Select(x => new GetDownloadAllUnexcusedAbsenceByPeriodResult
                            {
                                StudentId = x.GeneratedScheduleLesson.GeneratedScheduleStudent.IdStudent,
                                StudentName = x.GeneratedScheduleLesson.GeneratedScheduleStudent.Student.FirstName + " " + x.GeneratedScheduleLesson.GeneratedScheduleStudent.Student.LastName,
                                Class = x.GeneratedScheduleLesson.HomeroomName,
                                DateOfAbsence = x.GeneratedScheduleLesson.ScheduleDate,
                                ClassSession = x.GeneratedScheduleLesson.Session.SessionID.ToString(),
                                Subject = x.GeneratedScheduleLesson.SubjectName,
                                TeacherName = x.GeneratedScheduleLesson.TeacherName,
                                AttendanceStatus = x.AttendanceMappingAttendance.Attendance.Description
                            }).OrderBy(x => x.StudentId).ThenBy(x => x.DateOfAbsence).ToList();

            if(param.IsDailyAttendance == true)
            {
                resultAttendance.GroupBy(x => new { StudentId = x.StudentId, DateOfAbsence = x.DateOfAbsence }).ToList();
            }

            var result = await _dbContext.Entity<MsLevel>()
                                        .Include(x => x.AcademicYear)
                                        .Where(x => x.Id == param.IdLevel
                                                    && x.IdAcademicYear == param.IdAcademicYear)
                                        .Where(predicateLevel)
                                        .Where(x => x.Id == param.IdLevel)
                                        .Select(x => new GetSummaryDetailResult<SummaryByStudentResult>
                                        {
                                            AcademicYear = new CodeWithIdVm
                                            {
                                                Id = x.AcademicYear.Id,
                                                Code = x.AcademicYear.Code,
                                                Description = x.AcademicYear.Description
                                            },
                                            Level = new CodeWithIdVm
                                            {
                                                Id = x.Id,
                                                Code = x.Code,
                                                Description = x.Description
                                            }
                                        }).SingleOrDefaultAsync();
            if (result is null)
            {
                var dataGrade = await _dbContext.Entity<MsGrade>().Where(x => x.Id == param.IdGrade).FirstOrDefaultAsync(CancellationToken);
                var dataUser = await _dbContext.Entity<MsStaff>().Where(x => x.IdBinusian == param.IdUser).FirstOrDefaultAsync(CancellationToken);
                throw new NotFoundException($"{dataUser.FirstName} {dataUser.LastName} has no assign to {dataGrade.Description}");
            }

            var excelRecap = GenerateExcel(resultAttendance);

            return new FileContentResult(excelRecap, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Recap-All-Unexcused-Absence{DateTime.Now.Ticks}.xlsx"
            };
        }
        private byte[] GenerateExcel(List<GetDownloadAllUnexcusedAbsenceByPeriodResult> data)
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

            if(param.IsDailyAttendance == true)
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

                foreach(var itemData in data){
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
                    cellTeacher.SetCellValue(itemData.TeacherName);
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

                foreach(var itemData in data){
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
