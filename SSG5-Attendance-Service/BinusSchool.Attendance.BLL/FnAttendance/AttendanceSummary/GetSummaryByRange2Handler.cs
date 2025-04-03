using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryByRange2Handler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetSummaryByRangeRequest.IdAcademicYear),
            nameof(GetSummaryByRangeRequest.StartDate),
            nameof(GetSummaryByRangeRequest.EndDate),
            nameof(GetSummaryByRangeRequest.IdSchool),
            nameof(GetSummaryByRangeRequest.IdUser)
        });

        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetSummaryByRange2Handler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var sw = Stopwatch.StartNew();
            
            FillConfiguration();

            var param = Request.ValidateParams<GetSummaryByRangeRequest>(_requiredParams.Value);
            List<string> idLevelForSHAndSha = new List<string>();

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
            var positionUser = await _dbContext.Entity<TrNonTeachingLoad>().Include(x => x.NonTeachingLoad)
                .ThenInclude(x => x.TeacherPosition).ThenInclude(x => x.LtPosition)
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
            var predicateLesson = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => 1 == 1);
            var predicateStudentGrade = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => 1 == 1);
            List<string> idLevelPrincipalAndVicePrincipal = new List<string>();
            if (positionUser.Count > 0)
            {
                if (param.SelectedPosition == PositionConstant.Principal)
                {
                    if (positionUser.Any(y => y.Code == PositionConstant.Principal)) //check P Or VP
                    {
                        if (positionUser.Where(y => y.Code == PositionConstant.Principal).ToList() != null &&
                            positionUser.Where(y => y.Code == PositionConstant.Principal).Count() > 0)
                        {
                            var Principal = positionUser.Where(x => x.Code == PositionConstant.Principal).ToList();

                            foreach (var item in Principal)
                            {
                                var _dataNewLH =
                                    JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                                _dataNewLH.TryGetValue("Level", out var _levelLH);
                                idLevelPrincipalAndVicePrincipal.Add(_levelLH.Id);
                            }

                            predicateLevelPrincipalAndVicePrincipal = predicateLevelPrincipalAndVicePrincipal.And(x =>
                                idLevelPrincipalAndVicePrincipal.Contains(x.Grade.IdLevel));
                        }
                    }
                }

                if (param.SelectedPosition == PositionConstant.VicePrincipal)
                {
                    if (positionUser.Any(y => y.Code == PositionConstant.VicePrincipal))
                    {
                        if (positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).ToList() != null &&
                            positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).Count() > 0)
                        {
                            var Principal = positionUser.Where(x => x.Code == PositionConstant.VicePrincipal).ToList();
                            List<string> IdLevels = new List<string>();
                            foreach (var item in Principal)
                            {
                                var _dataNewLH =
                                    JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                                _dataNewLH.TryGetValue("Level", out var _levelLH);
                                IdLevels.Add(_levelLH.Id);
                            }

                            predicateLevelPrincipalAndVicePrincipal =
                                predicateLevelPrincipalAndVicePrincipal.And(x => IdLevels.Contains(x.Grade.IdLevel));
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
                            idLevelForSHAndSha.Add(_leveltSH.Id);
                            IdGrade.Add(_gradeSH.Id);
                            IdSubject.Add(_subjectSH.Id);
                        }

                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
                        predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
                        predicateLesson = predicateLesson.And(x => IdSubject.Contains(x.IdSubject));
                        //predicateStudentGrade = predicateStudentGrade.And(x => x.TrGeneratedScheduleStudent.MsStudent.StudentGrades.Any(g => IdGrade.Contains(g.IdGrade)));
                    }
                }

                if (param.SelectedPosition == PositionConstant.SubjectHeadAssitant)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.SubjectHeadAssitant).ToList() != null)
                    {
                        var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHeadAssitant)
                            .ToList();
                        List<string> IdGrade = new List<string>();
                        List<string> IdSubject = new List<string>();
                        foreach (var item in LevelHead)
                        {
                            var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewSH.TryGetValue("Level", out var _leveltSH);
                            _dataNewSH.TryGetValue("Grade", out var _gradeSH);
                            _dataNewSH.TryGetValue("Department", out var _departmentSH);
                            _dataNewSH.TryGetValue("Subject", out var _subjectSH);
                            idLevelForSHAndSha.Add(_leveltSH.Id);
                            IdGrade.Add(_gradeSH.Id);
                            IdSubject.Add(_subjectSH.Id);
                        }

                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => IdGrade.Contains(g.Id)));
                        predicateHomeroom = predicateHomeroom.And(x => IdGrade.Contains(x.IdGrade));
                        predicateLesson = predicateLesson.And(x => IdSubject.Contains(x.IdSubject));
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
                                    var gradePerLevel =
                                        subjectByDepartments.Where(x => x.IdLevel == departmentLevel.IdLevel);
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
                        predicateLesson = predicateLesson.And(x => IdSubject.Contains(x.IdSubject));
                        //predicateStudentGrade = predicateStudentGrade.And(x => x.TrGeneratedScheduleStudent.MsStudent.StudentGrades.Any(g => IdGrade.Contains(g.IdGrade)));
                    }
                }
            }
            else
            {
                if (param.SelectedPosition == PositionConstant.ClassAdvisor)
                {
                    if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
                        predicateHomeroom = PredicateBuilder.Create<MsHomeroom>(x =>
                            dataHomeroomTeacher.Contains(x.Id) &&
                            x.HomeroomTeachers.Any(ht => ht.IdBinusian == param.IdUser));
                }

                if (param.SelectedPosition == PositionConstant.SubjectTeacher)
                {
                    if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
                    {
                        predicateLesson = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x =>
                            dataLessonTeacher.Contains(x.IdLesson) && x.IdUser == param.IdUser);
                        predicateHomeroom = predicateHomeroom.And(x =>
                            x.HomeroomPathways.Any(y =>
                                y.LessonPathways.Any(z => dataLessonTeacher.Contains(z.IdLesson))));
                    }
                }
            }

            #endregion

            var homerooms = await _dbContext.Entity<MsHomeroom>()
                .Include(x => x.Grade).ThenInclude(x => x.Level)
                .Where(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                .Where(predicateHomeroom)
                .Where(predicateLevelPrincipalAndVicePrincipal)
                .Select(x => new
                {
                    x.Id,
                    x.Grade.IdLevel
                })
                .ToListAsync(CancellationToken);

            var idHomerooms = homerooms.Select(x => x.Id).Distinct().ToList();
            List<string> allowIsAttendanceEntryByClassId = new List<string>();
            if (param.SelectedPosition == PositionConstant.SubjectTeacher)
            {
                var dataLesson = await _dbContext.Entity<MsLessonTeacher>()
                    .Include(x => x.Lesson)
                    .Where(x => x.IdUser == param.IdUser)
                    .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear)
                    .Where(x => x.IsAttendance)
                    .Select(x => new
                    {
                        x.Lesson.Id,
                        x.Lesson.Grade.IdLevel
                    })
                    .ToListAsync(CancellationToken);
                allowIsAttendanceEntryByClassId = dataLesson.Select(x => x.Id).ToList();
                predicateLesson = predicateLesson.And(x => allowIsAttendanceEntryByClassId.Contains(x.IdLesson));
                var _idLevel = dataLesson.Select(x => x.IdLevel).ToList();
                var idLevelAllowed = _dbContext.Entity<MsMappingAttendance>()
                    .Where(x => _idLevel.Contains(x.IdLevel) && x.AbsentTerms == AbsentTerm.Session)
                    .Select(x => x.IdLevel).ToList();
                predicateLesson = predicateLesson.And(x => idLevelAllowed.Contains(x.Homeroom.Grade.IdLevel));
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

                predicateLesson = predicateLesson.And(x => allowIsAttendanceEntryByClassId.Contains(x.IdHomeroom));
            }

            if (param.SelectedPosition == PositionConstant.SubjectHead ||
                param.SelectedPosition == PositionConstant.SubjectHeadAssitant)
            {
                var idLevelAllowed = await _dbContext.Entity<MsMappingAttendance>()
                    .Where(x => idLevelForSHAndSha.Contains(x.IdLevel) && x.AbsentTerms == AbsentTerm.Session)
                    .Select(x => x.IdLevel).ToListAsync(CancellationToken);
                predicateLesson = predicateLesson.And(x => idLevelAllowed.Contains(x.Homeroom.Grade.IdLevel));
            }

            var allSchedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Include(x => x.Homeroom)
                .ThenInclude(e => e.Grade)
                .Where(x =>
                    x.IsGenerated
                    && x.ScheduleDate >= param.StartDate
                    && x.ScheduleDate <= param.EndDate
                    && idHomerooms.Contains(x.IdHomeroom)
                )
                .Where(predicateLesson)
                .Where(predicateStudentGrade)
                .Include(x => x.GeneratedScheduleStudent)
                .Include(x => x.AttendanceEntries)
                .Select(e => new
                {
                    e.Homeroom.Grade.IdLevel,
                    e.IdHomeroom,
                    e.IdGeneratedScheduleStudent,
                    e.GeneratedScheduleStudent.IdStudent,
                    e.ScheduleDate,
                    e.ClassID,
                    e.IdSession,
                    e.IdLesson,
                    Items = e.AttendanceEntries.Select(x => x.Status)
                })
                .ToListAsync(CancellationToken);

            var idLevels = allSchedules.Select(x => x.IdLevel).Distinct().ToList();
            var result = await _dbContext.Entity<MsLevel>()
                .Include(x => x.AcademicYear)
                .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                .Where(predicateLevel)
                .Where(x => idLevels.Contains(x.Id))
                .Select(x => new SummaryResult
                {
                    Period = new DateTimeRange(param.StartDate, param.EndDate),
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
                })
                .ToListAsync(CancellationToken);

            var mappings = await _dbContext.Entity<MsMappingAttendance>()
                .Where(x => idLevels.Contains(x.IdLevel))
                .ToListAsync(CancellationToken);
            foreach (var item in result)
            {
                var mappingLevel = mappings.FirstOrDefault(x => x.IdLevel == item.Level.Id);
                if (mappingLevel == null)
                    throw new BadRequestException($"{item.Level.Description} doesn't have mapping yet");

                var homeroomIds = homerooms.Where(x => x.IdLevel == item.Level.Id)
                    .Select(x => x.Id)
                    .ToList();

                var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x =>
                        new
                        {
                            x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus,
                            x.IdAcademicYear
                        })
                    .Where(x => x.IdAcademicYear == param.IdAcademicYear && (x.StartDate == param.StartDate.Date ||
                                                                             x.EndDate == param.EndDate.Date
                                                                             || (x.StartDate < param.StartDate.Date
                                                                                 ? x.EndDate != null
                                                                                     ? (x.EndDate >
                                                                                         param.StartDate.Date &&
                                                                                         x.EndDate < param.EndDate
                                                                                             .Date) || x.EndDate >
                                                                                     param.EndDate.Date
                                                                                     : x.StartDate <=
                                                                                     param.StartDate.Date
                                                                                 : x.EndDate != null
                                                                                     ? ((param.EndDate.Date >
                                                                                             x.StartDate &&
                                                                                             param.EndDate.Date <
                                                                                             x.EndDate) ||
                                                                                         param.EndDate.Date > x.EndDate)
                                                                                     : x.StartDate <=
                                                                                     param.StartDate.Date)) &&
                                x.CurrentStatus == "A" && x.ActiveStatus == false)
                    .ToListAsync();

                List<string> studentId = new List<string>();

                List<DateTime> studentDateNonActive = new List<DateTime>();

                foreach (var studentStatus in checkStudentStatus)
                {
                    if (studentStatus.EndDate != null)
                    {
                        if (param.EndDate <= studentStatus.EndDate)
                        {
                            studentId.Add(studentStatus.IdStudent);
                            studentDateNonActive.Add(studentStatus.StartDate);
                        }
                    }
                    else
                    {
                        studentId.Add(studentStatus.IdStudent);
                        studentDateNonActive.Add(studentStatus.StartDate);
                    }
                }

                if (checkStudentStatus != null)
                {
                    if (studentId.Any())
                        allSchedules = allSchedules
                            .Where(x => !studentId.ToList().Contains(x.IdStudent)).ToList();
                }

                var schedules = allSchedules.Where(x => homeroomIds.Contains(x.IdHomeroom)).ToList();
                var schedulessubmittedList = allSchedules
                    .Where(x => homeroomIds.Contains(x.IdHomeroom) && x.Items.Count() != 0).ToList();
                var schedulesUnsubmittedList = allSchedules
                    .Where(x => homeroomIds.Contains(x.IdHomeroom) && x.Items.Count() == 0).ToList();

                var schedulesUnsubmitted = new List<TempModel>();

                foreach (var dataUnsubmitted in schedulesUnsubmittedList)
                {
                    if (mappingLevel.AbsentTerms == AbsentTerm.Session)
                    {
                        if (schedulessubmittedList.Any(x =>
                                x.ScheduleDate == dataUnsubmitted.ScheduleDate &&
                                x.IdHomeroom == dataUnsubmitted.IdHomeroom &&
                                x.IdSession == dataUnsubmitted.IdSession &&
                                x.IdGeneratedScheduleStudent == dataUnsubmitted.IdGeneratedScheduleStudent &&
                                x.ClassID == dataUnsubmitted.ClassID && x.IdLesson == dataUnsubmitted.IdLesson) ==
                            false)
                        {
                            var temp = new TempModel();
                            temp.ScheduleDate = dataUnsubmitted.ScheduleDate;
                            temp.ClassID = dataUnsubmitted.ClassID;
                            temp.IdStudent = dataUnsubmitted.IdStudent;
                            temp.IdHomeroom = dataUnsubmitted.IdHomeroom;
                            temp.IdSession = dataUnsubmitted.IdSession;
                            temp.Items.AddRange(dataUnsubmitted.Items.Select(e => e.ToString()));
                            schedulesUnsubmitted.Add(temp);
                        }
                    }
                    else
                    {
                        if (schedulessubmittedList.Any(x =>
                                x.ScheduleDate == dataUnsubmitted.ScheduleDate &&
                                x.IdHomeroom == dataUnsubmitted.IdHomeroom &&
                                x.IdGeneratedScheduleStudent == dataUnsubmitted.IdGeneratedScheduleStudent) == false)
                        {
                            var temp = new TempModel();
                            temp.ScheduleDate = dataUnsubmitted.ScheduleDate;
                            temp.ClassID = dataUnsubmitted.ClassID;
                            temp.IdStudent = dataUnsubmitted.IdStudent;
                            temp.IdHomeroom = dataUnsubmitted.IdHomeroom;
                            temp.IdSession = dataUnsubmitted.IdSession;
                            temp.Items.AddRange(dataUnsubmitted.Items.Select(e => e.ToString()));
                            schedulesUnsubmitted.Add(temp);
                        }
                    }
                }

                item.AbsentTerm = mappingLevel.AbsentTerms;
                item.IsNeedValidation = mappingLevel.IsNeedValidation;
                item.TotalStudent = schedules.GroupBy(x => x.IdStudent).Count();
                item.Submitted = mappingLevel.AbsentTerms == AbsentTerm.Session
                    ? schedules
                        .Where(x => x.Items.Any(y => y == AttendanceEntryStatus.Submitted))
                        .GroupBy(x => new { x.ScheduleDate, x.IdSession }).Count()
                    : schedules.Where(x => x.Items.Any(y => y == AttendanceEntryStatus.Submitted))
                        .GroupBy(x => new { x.ScheduleDate }).Count();
                item.Pending = mappingLevel.AbsentTerms == AbsentTerm.Session
                    ? schedules.Where(x => x.Items.Any(y => y == AttendanceEntryStatus.Pending))
                        .GroupBy(x => new { x.ScheduleDate, x.ClassID, x.IdSession })
                        .Count()
                    : schedules.Where(x => x.Items.Any(y => y == AttendanceEntryStatus.Pending))
                        .GroupBy(x => new { x.ScheduleDate, x.IdHomeroom })
                        .Count();
                item.Unsubmitted = mappingLevel.AbsentTerms == AbsentTerm.Session ? checkStudentStatus == null
                        ? schedulesUnsubmitted.Where(x => !x.Items.Any()
                                                          && x.ScheduleDate <= _dateTime.ServerTime.Date)
                            .GroupBy(x => new { x.ScheduleDate, x.ClassID, x.IdSession })
                            .Count()
                        : schedulesUnsubmitted.Where(x => !x.Items.Any()
                                                          && !checkStudentStatus.Select(y => y.IdStudent).ToList()
                                                              .Contains(x.IdStudent)
                                                          && x.ScheduleDate <= _dateTime.ServerTime.Date)
                            .GroupBy(x => new { x.ScheduleDate, x.ClassID, x.IdSession })
                            .Count() :
                    checkStudentStatus == null ? schedulesUnsubmitted.Where(x => !x.Items.Any()
                            && x.ScheduleDate <= _dateTime.ServerTime.Date)
                        .GroupBy(x => new { x.ScheduleDate, x.IdHomeroom })
                        .Count() : schedulesUnsubmitted.Where(x => !x.Items.Any()
                                                                   && !checkStudentStatus.Select(y => y.IdStudent)
                                                                       .ToList().Contains(x.IdStudent)
                                                                   && x.ScheduleDate <= _dateTime.ServerTime.Date)
                        .GroupBy(x => new { x.ScheduleDate, x.IdHomeroom })
                        .Count();
            }

            sw.Stop();
            
            Logger.LogInformation("by range new takes {Seconds}", Math.Round(sw.Elapsed.TotalSeconds, 2));
            
            return Request.CreateApiResult2(result as object);
        }
    }

    public class TempModel
    {
        public TempModel()
        {
            Items = new List<string>();
        }

        public DateTime ScheduleDate { get; set; }
        public string ClassID { get; set; }
        public string IdStudent { get; set; }
        public string IdHomeroom { get; set; }
        public string IdSession { get; set; }
        public List<string> Items { get; set; }
    }
}
