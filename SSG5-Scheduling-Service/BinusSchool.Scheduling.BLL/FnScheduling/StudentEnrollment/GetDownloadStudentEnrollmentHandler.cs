using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollment
{
    public class GetDownloadStudentEnrollmentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetDownloadStudentEnrollmentHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<GetStudentEnrollmentRequest>(nameof(GetStudentEnrollmentRequest.IdHomeroom));
            
            var homeroom = await _dbContext.Entity<MsHomeroom>()
                .FindAsync(new[] { param.IdHomeroom }, CancellationToken);

            if (homeroom is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Homeroom"], "Id", param.IdHomeroom));

            var students = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(e => e.Student)
                .Where(x => x.IdHomeroom == param.IdHomeroom)
                .Select(x => new
                {
                    IdHomeroomStudent = x.Id,
                    IdStudent = x.Student.Id,
                    Religion = x.Student.Religion.ReligionName,
                    StudentName = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)
                })
                .ToListAsync(CancellationToken);

            students = students.OrderBy(x => x.StudentName).ToList();

            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new { x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus })
            .Where(x => (x.StartDate == _dateTime.ServerTime.Date || x.EndDate == _dateTime.ServerTime.Date
                || (x.StartDate < _dateTime.ServerTime.Date
                    ? x.EndDate != null ? (x.EndDate > _dateTime.ServerTime.Date && x.EndDate < _dateTime.ServerTime.Date) || x.EndDate > _dateTime.ServerTime.Date : x.StartDate <= _dateTime.ServerTime.Date
                    : x.EndDate != null ? ((_dateTime.ServerTime.Date > x.StartDate && _dateTime.ServerTime.Date < x.EndDate) || _dateTime.ServerTime.Date > x.EndDate) : x.StartDate <= _dateTime.ServerTime.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
            .ToListAsync();

            if (checkStudentStatus != null)
            {
                students = students.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).ToList();
            }

            var lessons = await _dbContext.Entity<MsLessonPathway>()
                .Where(x => x.HomeroomPathway.IdHomeroom == param.IdHomeroom && x.Lesson.Semester == homeroom.Semester)
                .Select(x => x.Lesson)
                .Distinct()
                .ToListAsync(CancellationToken);

            var enrollments = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Where(x => x.HomeroomStudent.IdHomeroom == param.IdHomeroom)
                .Select(x => new
                {
                    x.Id,
                    x.IdLesson,
                    x.IdHomeroomStudent,
                    x.IdSubjectLevel
                })
                .ToListAsync(CancellationToken);

            #region GetLevelAndAcademicYear
            var getLevelAy = await _dbContext.Entity<MsHomeroom>()
                            .Include(x => x.Grade)
                                .ThenInclude(x => x.Level)
                            .Include(x => x.AcademicYear)
                            .Where(x => x.Id == param.IdHomeroom)
                            .Select(x => new
                            {
                                level = x.Grade.Level.Description,
                                academicYear = x.AcademicYear.Description
                            })
                            .FirstOrDefaultAsync(CancellationToken);
            #endregion

            #region GetClassroomMappedsByGrade
            var ListIdGrade = new[] { homeroom.IdGrade };
            var crMapResult = await _dbContext.Entity<MsGradePathwayClassroom>()
                .Include(x => x.Classroom)
                .Include(x => x.MsGradePathway).ThenInclude(x => x.GradePathwayDetails).ThenInclude(x => x.Pathway)
                .Include(x => x.MsGradePathway).ThenInclude(x => x.Grade)
                .Where(x => x.MsGradePathway.IdGrade == homeroom.IdGrade)
                .Select(x => new GetClassroomMapByGradeResult
                {
                    Id = x.Id,
                    Code = x.Classroom.Code,
                    Description = x.Classroom.Description,
                    Formatted = $"{x.MsGradePathway.Grade.Code}{x.Classroom.Code}",
                    Grade = new CodeWithIdVm
                    {
                        Id = x.MsGradePathway.IdGrade,
                        Code = x.MsGradePathway.Grade.Code,
                        Description = x.MsGradePathway.Grade.Description
                    },
                    Pathway = new ClassroomMapPathway
                    {
                        Id = x.MsGradePathway.Id,
                        PathwayDetails = x.MsGradePathway.GradePathwayDetails.Select(y => new CodeWithIdVm
                        {
                            Id = y.Id,
                            Code = y.Pathway.Code,
                            Description = y.Pathway.Description
                        })
                    },
                    Class = new CodeWithIdVm
                    {
                        Id = x.Classroom.Id,
                        Code = x.Classroom.Code,
                        Description = x.Classroom.Description
                    }
                })
                .OrderBy(x => x.Grade.Code).ThenBy(x => x.Code)
                .ToListAsync(CancellationToken);
            #endregion

            #region GetSubject
            var ListIdSubject = lessons.Select(x => x.IdSubject);

            var result = await _dbContext.Entity<MsSubject>()
                    .Include(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear)
                    .Include(x => x.SubjectMappingSubjectLevels).ThenInclude(x => x.SubjectLevel)
                    .Include(x => x.Department)
                .Where(e => ListIdSubject.Contains(e.Id))

                .ToListAsync();
            var idSubjectGroups = result.Where(x => x.IdSubjectGroup != null).Select(x => x.IdSubjectGroup).Distinct().ToArray();
            var existSubjectGroups = new List<MsSubjectGroup>();
            if (idSubjectGroups.Length != 0)
            {
                existSubjectGroups = await _dbContext
                    .Entity<MsSubjectGroup>().Where(x => idSubjectGroups.Contains(x.Id))
                    .ToListAsync(CancellationToken);
            }

            var subjectResult = result
                                .Select(x => new GetSubjectResult
                                {
                                    Id = x.Id,
                                    Code = x.Code,
                                    Description = x.Description,
                                    Grade = x.Grade.Description,
                                    Acadyear = x.Grade.Level.AcademicYear.Code,
                                    IdDepartment = x.IdDepartment,
                                    Department = x.Department.Description,
                                    SubjectId = x.SubjectID,
                                    SubjectLevel = x.SubjectMappingSubjectLevels.Count != 0 // force order asc one-to-many relation
                                            ? string.Join(", ", x.SubjectMappingSubjectLevels
                                                .OrderBy(y => y.SubjectLevel.Code).Select(y => y.SubjectLevel.Code))
                                            : Localizer["General"],
                                    SubjectLevels = x.SubjectMappingSubjectLevels.Count != 0 // force order asc one-to-many relation
                                            ? x.SubjectMappingSubjectLevels
                                                .OrderBy(y => y.SubjectLevel.Code)
                                                .Select(y => new SubjectLevelResult
                                                {
                                                    Id = y.SubjectLevel.Id,
                                                    Code = y.SubjectLevel.Code,
                                                    Description = y.SubjectLevel.Description,
                                                    IsDefault = y.IsDefault
                                                })
                                            : null,
                                    SubjectGroup = x.IdSubjectGroup != null && existSubjectGroups.Any(y => y.Id == x.IdSubjectGroup)
                                            ? new CodeWithIdVm
                                            {
                                                Id = x.IdSubjectGroup,
                                                Code = existSubjectGroups.Find(y => y.Id == x.IdSubjectGroup).Code,
                                                Description = existSubjectGroups.Find(y => y.Id == x.IdSubjectGroup).Description
                                            }
                                            : null,
                                    MaxSession = x.MaxSession
                                }).ToList();

            #endregion

            #region GetStudentsByGrade

            var ListIdUser = students.Select(x => x.IdStudent);
            var predicate = PredicateBuilder.Create<MsStudentGrade>(x => ListIdGrade.Contains(x.IdGrade));
            if (ListIdUser?.Any() ?? false)
                predicate = predicate.And(x => ListIdUser.Contains(x.IdStudent));

            var queryStudent = _dbContext.Entity<MsStudentGrade>()
                .Include(x => x.Student)
                .Include(x => x.StudentGradePathways)
                .Where(predicate)
                .IgnoreQueryFilters();

            var gradeResult = await _dbContext.Entity<MsGrade>()
                .Include(x => x.Level)
                .ThenInclude(x => x.AcademicYear.School)
                .Select(x => new GetGradeDetailResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                })
                .FirstOrDefaultAsync(x => ListIdGrade.Contains(x.Id), CancellationToken);

            var studentResult = queryStudent
                    .Select(x => new GetStudentByGradeResult
                    {
                        Id = x.Id,
                        StudentId = x.IdStudent,
                        Grade = gradeResult.Description,
                        FullName = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                        Gender = x.Student.Gender,
                        IsActive = x.IsActive,
                    })
                    .ToList();
            #endregion

            var availableLessons = lessons
                .Join(subjectResult, lesson => lesson.IdSubject, subject => subject.Id, (lesson, subject) => (lesson, subject))
                .OrderByDescending(x => !string.IsNullOrEmpty(x.subject?.SubjectGroup?.Id))
                    .ThenBy(x => x.subject?.SubjectGroup?.Id)
                    .ThenBy(x => x.lesson.ClassIdGenerated);

            var rLessons = new List<IDictionary<string, object>>();
            foreach (var student in students)
            {
                var currentStudent = studentResult.FirstOrDefault(x => x.StudentId == student.IdStudent);
                var rLesson = new Dictionary<string, object>
                {
                    { "Student Name", student.StudentName },
                    { "Religion", student.Religion },
                    { "Total Subject", null}
                };

                foreach (var (lesson, subject) in availableLessons)
                {
                    // find enrolled student to lesson
                    var currentEnrollment = enrollments.Find(x => x.IdLesson == lesson.Id && x.IdHomeroomStudent == student.IdHomeroomStudent);

                    var rSubjectLevels = subject.SubjectLevels?.Select(x => new
                    {
                        id = x.Id,
                        code = x.Code,
                        isDefault = x.IsDefault,
                        isTick = currentEnrollment != null && currentEnrollment.IdSubjectLevel == x.Id
                    });
                    var rSubject = new
                    {
                        idEnrollment = currentEnrollment?.Id,
                        idLesson = lesson.Id,
                        code = subject?.Code,
                        group = subject?.SubjectGroup?.Id,
                        isTick = currentEnrollment != null,
                        subjectId = subject?.Id
                    };

                    if (!rLesson.Keys.Any(x => x == lesson.ClassIdGenerated))
                        rLesson.Add(lesson.ClassIdGenerated, rSubject);
                }

                rLessons.Add(rLesson);
            }

            var currentCrMap = crMapResult.FirstOrDefault(x => homeroom.IdGradePathwayClassRoom == x.Id);
            var _result = new GetStudentEnrollmentResult
            {
                Grade = new CodeWithIdVm
                {
                    Id = homeroom.IdGrade,
                    Code = currentCrMap?.Grade?.Code,
                    Description = currentCrMap?.Grade?.Description
                },
                Homeroom = new CodeWithIdVm
                {
                    Id = param.IdHomeroom,
                    Code = currentCrMap?.Code,
                    Description = currentCrMap?.Description
                },
                Semester = homeroom.Semester,
                LessonGroups = availableLessons.Any(x => x.subject.SubjectGroup != null)
                    ? availableLessons
                        .Where(x => x.subject.SubjectGroup != null)
                        .GroupBy(x => x.subject.SubjectGroup.Id, x => x.subject.SubjectGroup)
                        .Select(x => x.First()).ToList()
                    : null,
                Lessons = rLessons
            };


            var generateExcel = GenerateExcel(_result, getLevelAy.level, getLevelAy.academicYear);

            return new FileContentResult(generateExcel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Acaademic_Year_Data_Setting_Student_Enrollment_{DateTime.Now.Ticks}.xlsx"
            };
        }
        private byte[] GenerateExcel(GetStudentEnrollmentResult studentEnrollment, string level, string academicYear)
        {
            var workbook = new XSSFWorkbook();

            #region styling doc 
            var fontBold = workbook.CreateFont();
            fontBold.IsBold = true;

            var cellStyle = workbook.CreateCellStyle();
            cellStyle.BorderRight = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;

            var boldStyle = workbook.CreateCellStyle();
            boldStyle.SetFont(fontBold);

            var cellStyleAndBold = workbook.CreateCellStyle();
            cellStyleAndBold.BorderRight = BorderStyle.Thin;
            cellStyleAndBold.BorderLeft = BorderStyle.Thin;
            cellStyleAndBold.BorderBottom = BorderStyle.Thin;
            cellStyleAndBold.BorderTop = BorderStyle.Thin;
            cellStyleAndBold.SetFont(fontBold);
            cellStyleAndBold.Alignment = HorizontalAlignment.Center;
            #endregion

            var sheet = workbook.CreateSheet();

            var rowAyHeader = sheet.CreateRow(0);
            var cellAy = rowAyHeader.CreateCell(0);
            var cellAyValue = rowAyHeader.CreateCell(1);
            cellAy.SetCellValue("Academic Year");
            cellAy.CellStyle = boldStyle;
            cellAyValue.SetCellValue(academicYear);

            var rowSemesterHeader = sheet.CreateRow(1);
            var cellSemester = rowSemesterHeader.CreateCell(0);
            var cellSemesterValue = rowSemesterHeader.CreateCell(1);
            cellSemester.SetCellValue("Semester");
            cellSemester.CellStyle = boldStyle;
            cellSemesterValue.SetCellValue(studentEnrollment.Semester);

            var rowLevelHeader = sheet.CreateRow(2);
            var cellLevel = rowLevelHeader.CreateCell(0);
            var cellLevelValue = rowLevelHeader.CreateCell(1);
            cellLevel.SetCellValue("Level");
            cellLevel.CellStyle = boldStyle;
            cellLevelValue.SetCellValue(level);

            var rowGradeHeader = sheet.CreateRow(3);
            var cellGrade = rowGradeHeader.CreateCell(0);
            var cellGradeValue = rowGradeHeader.CreateCell(1);
            cellGrade.SetCellValue("Grade");
            cellGrade.CellStyle = boldStyle;
            cellGradeValue.SetCellValue(studentEnrollment.Grade.Code);

            var rowHomeroomHeader = sheet.CreateRow(4);
            var cellHomeroom = rowHomeroomHeader.CreateCell(0);
            var cellHomeroomValue = rowHomeroomHeader.CreateCell(1);
            cellHomeroom.SetCellValue("Homeroom");
            cellHomeroom.CellStyle = boldStyle;
            cellHomeroomValue.SetCellValue($"{studentEnrollment.Grade.Code}{studentEnrollment.Homeroom.Description}");

            #region header column table
            var lesson = studentEnrollment.Lessons.FirstOrDefault();

            var rowHeader = sheet.CreateRow(6);
            int cell = 0;
            foreach (var item in lesson)
            {
                var cellName = rowHeader.CreateCell(cell);

                cellName.SetCellValue(item.Key);

                cellName.CellStyle = cellStyleAndBold;

                cell++;
            }
            #endregion

            #region list data student enrollment
            int rowIndex = 7;
            int startColumn = 0;
            foreach (var lessonData in studentEnrollment.Lessons)
            {
                rowHeader = sheet.CreateRow(rowIndex);
                foreach (var item in lessonData )
                {
                    sheet.AutoSizeColumn(startColumn);
                    var cellLesson = rowHeader.CreateCell(startColumn);
                    var value = "";

                    if (item.Key.ToLower() == "student name" || item.Key.ToLower() == "religion")
                    {
                        value = item.Value != null ? item.Value.ToString() : "";
                    }
                    else if (item.Key.ToLower() == "total subject")
                    {
                        var total = 0;

                        foreach (var itemlessonData in lessonData)
                        {
                            if (itemlessonData.Key.ToLower() != "student name" && itemlessonData.Key.ToLower() != "religion" && itemlessonData.Key.ToLower() != "total subject")
                            {
                                var json = JsonConvert.SerializeObject(itemlessonData.Value);
                                var dictionaries = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                                foreach (var itemDictonary in dictionaries)
                                {
                                    if (itemDictonary.Key.ToLower() == "istick")
                                    {
                                        if (itemDictonary.Value.ToString() == "true")
                                        {
                                            total++;
                                        }
                                    }
                                }
                            }
                        }
                        value = total.ToString();
                    }
                    else
                    {
                        var json = JsonConvert.SerializeObject(item.Value);
                        var dictionaries = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                        foreach (var itemDictonary in dictionaries)
                        {
                            if (itemDictonary.Key.ToLower() == "istick")
                            {
                                value = itemDictonary.Value.ToString() == "true" ? "Yes" : "-";
                            }
                        }

                    }

                    cellLesson.SetCellValue(value);
                    cellLesson.CellStyle = cellStyle;

                    startColumn++;
                }
                rowIndex++;
                startColumn = 0;
            }
            #endregion


            using var ms = new MemoryStream();

            workbook.Write(ms);

            return ms.ToArray();
        }
    }
}
