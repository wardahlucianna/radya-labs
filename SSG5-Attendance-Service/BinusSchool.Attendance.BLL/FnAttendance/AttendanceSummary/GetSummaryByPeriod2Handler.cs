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
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable EqualExpressionComparison

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryByPeriod2Handler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetSummaryByPeriodRequest.IdAcademicYear),
            nameof(GetSummaryByPeriodRequest.Semester)
        });

        public GetSummaryByPeriod2Handler(IAttendanceDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var sw = Stopwatch.StartNew();

            FillConfiguration();

            var param = Request.ValidateParams<GetSummaryByPeriodRequest>(_requiredParams.Value);

            #region Get Position User

            var avaiablePosition = new List<string>();

            var idLevelForShAndSha = new List<string>();

            var dataHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                .Include(x => x.Homeroom)
                .Where(x => x.IdBinusian == param.IdUser && x.Homeroom.IdAcademicYear == param.IdAcademicYear)
                .Select(x => x.Homeroom.Id)
                .Distinct().ToListAsync(CancellationToken);

            var dataLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                .Include(x => x.Lesson)
                .Where(x => x.IdUser == param.IdUser)
                .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear)
                .Select(x => x.IdLesson)
                .Distinct()
                .ToListAsync(CancellationToken);

            var positionUser = await _dbContext.Entity<TrNonTeachingLoad>().Include(x => x.NonTeachingLoad)
                .ThenInclude(x => x.TeacherPosition).ThenInclude(x => x.LtPosition)
                .Where(x => x.NonTeachingLoad.IdAcademicYear == param.IdAcademicYear && x.IdUser == param.IdUser)
                .Select(x => new
                {
                    x.Data,
                    x.NonTeachingLoad.TeacherPosition.LtPosition.Code
                })
                .ToListAsync(CancellationToken);

            if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
                avaiablePosition.Add(PositionConstant.ClassAdvisor);

            if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
                avaiablePosition.Add(PositionConstant.SubjectTeacher);

            foreach (var pu in positionUser)
                avaiablePosition.Add(pu.Code);

            var predicateLevel = PredicateBuilder.Create<MsLevel>(x => 1 == 1);
            var predicateLevelPrincipalAndVicePrincipal = PredicateBuilder.Create<MsHomeroom>(x => 1 == 1);
            var predicateHomeroom = PredicateBuilder.Create<MsHomeroom>(x => 1 == 1);
            var predicateLesson = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => 1 == 1);
            var predicateStudentGrade = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => 1 == 1);
            var idLevelPrincipalAndVicePrincipal = new List<string>();
            if (positionUser.Count > 0)
            {
                if (param.SelectedPosition == PositionConstant.Principal)
                {
                    if (positionUser.Any(y => y.Code == PositionConstant.Principal)) //check P Or VP
                    {
                        var principals = positionUser.Where(x => x.Code == PositionConstant.Principal).ToList();

                        foreach (var item in principals)
                        {
                            var dataNewLh =
                                JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            dataNewLh.TryGetValue("Level", out var levelLh);
                            idLevelPrincipalAndVicePrincipal.Add(levelLh.Id);
                        }

                        predicateLevelPrincipalAndVicePrincipal = predicateLevelPrincipalAndVicePrincipal.And(x =>
                            idLevelPrincipalAndVicePrincipal.Contains(x.Grade.IdLevel));
                    }
                }

                if (param.SelectedPosition == PositionConstant.VicePrincipal)
                {
                    if (positionUser.Any(y => y.Code == PositionConstant.VicePrincipal))
                    {
                        var vicePrincipals = positionUser.Where(x => x.Code == PositionConstant.VicePrincipal)
                            .ToList();
                        var idLevelss = new List<string>();
                        foreach (var item in vicePrincipals)
                        {
                            var dataNewLh =
                                JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            dataNewLh.TryGetValue("Level", out var levelLh);
                            idLevelss.Add(levelLh.Id);
                        }

                        predicateLevelPrincipalAndVicePrincipal =
                            predicateLevelPrincipalAndVicePrincipal.And(x => idLevelss.Contains(x.Grade.IdLevel));
                    }
                }

                if (param.SelectedPosition == PositionConstant.LevelHead)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.LevelHead).ToList() != null)
                    {
                        var listLevelHead = positionUser.Where(x => x.Code == PositionConstant.LevelHead).ToList();
                        var idGrade = new List<string>();
                        foreach (var item in listLevelHead)
                        {
                            var dataNewLh = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            if (dataNewLh.TryGetValue("Grade", out var gradeLh))
                                idGrade.Add(gradeLh.Id);
                        }

                        predicateHomeroom = predicateHomeroom.And(x => idGrade.Contains(x.IdGrade));
                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => idGrade.Contains(g.Id)));
                    }
                }

                if (param.SelectedPosition == PositionConstant.SubjectHead)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.SubjectHead).ToList() != null)
                    {
                        var listSubjectHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHead).ToList();
                        var idGrade = new List<string>();
                        var idSubject = new List<string>();
                        foreach (var item in listSubjectHead)
                        {
                            var dataNewSh = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            if (dataNewSh.TryGetValue("Level", out var levelSh))
                                idLevelForShAndSha.Add(levelSh.Id);

                            if (dataNewSh.TryGetValue("Grade", out var gradeSh))
                                idGrade.Add(gradeSh.Id);

                            if (dataNewSh.TryGetValue("Subject", out var subjectSh))
                                idSubject.Add(subjectSh.Id);
                        }

                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => idGrade.Contains(g.Id)));
                        predicateHomeroom = predicateHomeroom.And(x => idGrade.Contains(x.IdGrade));
                        predicateLesson = predicateLesson.And(x => idSubject.Contains(x.IdSubject));
                    }
                }

                if (param.SelectedPosition == PositionConstant.SubjectHeadAssitant)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.SubjectHeadAssitant).ToList() != null)
                    {
                        var listSubjectHeadAssistant = positionUser
                            .Where(x => x.Code == PositionConstant.SubjectHeadAssitant)
                            .ToList();
                        var idGrade = new List<string>();
                        var idSubject = new List<string>();
                        foreach (var item in listSubjectHeadAssistant)
                        {
                            var dataNewSh = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            if (dataNewSh.TryGetValue("Level", out var levelSh))
                                idLevelForShAndSha.Add(levelSh.Id);

                            if (dataNewSh.TryGetValue("Grade", out var gradeSh))
                                idGrade.Add(gradeSh.Id);

                            if (dataNewSh.TryGetValue("Subject", out var subjectSh))
                                idSubject.Add(subjectSh.Id);
                        }

                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => idGrade.Contains(g.Id)));
                        predicateHomeroom = predicateHomeroom.And(x => idGrade.Contains(x.IdGrade));
                        predicateLesson = predicateLesson.And(x => idSubject.Contains(x.IdSubject));
                    }
                }

                if (param.SelectedPosition == PositionConstant.HeadOfDepartment)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.HeadOfDepartment).ToList() != null)
                    {
                        var listHeadOfDepartment = positionUser.Where(x => x.Code == PositionConstant.HeadOfDepartment)
                            .ToList();

                        var idDepartment = new List<string>();
                        var idGrade = new List<string>();
                        var idSubject = new List<string>();

                        foreach (var item in listHeadOfDepartment)
                        {
                            var dataNewSh = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            dataNewSh.TryGetValue("Department", out var departmentSh);
                            idDepartment.Add(departmentSh.Id);
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
                                        idGrade.Add(grade.IdGrade);
                                        idSubject.Add(grade.Id);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var item in subjectByDepartments)
                                {
                                    idGrade.Add(item.IdGrade);
                                    idSubject.Add(item.Id);
                                }
                            }
                        }

                        predicateLevel = predicateLevel.And(x => x.Grades.Any(g => idGrade.Contains(g.Id)));
                        predicateHomeroom = predicateHomeroom.And(x => idGrade.Contains(x.IdGrade));
                        predicateLesson = predicateLesson.And(x => idSubject.Contains(x.IdSubject));
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
                .Where(x => x.Semester == param.Semester)
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

            List<string> allowIsAttendanceEntryByClassId;

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
                var id = allowIsAttendanceEntryByClassId;
                predicateLesson = predicateLesson.And(x => id.Contains(x.IdLesson));
                var idLevel = dataLesson.Select(x => x.IdLevel).ToList();
                var idLevelAllowed = _dbContext.Entity<MsMappingAttendance>()
                    .Where(x => idLevel.Contains(x.IdLevel) && x.AbsentTerms == AbsentTerm.Session)
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
                var idLevelAllowed = _dbContext.Entity<MsMappingAttendance>()
                    .Where(x => idLevelForShAndSha.Contains(x.IdLevel) && x.AbsentTerms == AbsentTerm.Session)
                    .Select(x => x.IdLevel).ToList();
                predicateLesson = predicateLesson.And(x => idLevelAllowed.Contains(x.Homeroom.Grade.IdLevel));
            }

            var allSchedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Include(x => x.Homeroom)
                .ThenInclude(e => e.Grade)
                .Include(x => x.GeneratedScheduleStudent)
                .Include(x => x.AttendanceEntries)
                .Where(x => x.IsGenerated && idHomerooms.Contains(x.IdHomeroom))
                .Where(predicateLesson)
                .Where(predicateStudentGrade)
                .Select(e => new
                {
                    e.Homeroom.Grade.IdLevel,
                    e.IdHomeroom,
                    e.GeneratedScheduleStudent.IdStudent,
                    e.ScheduleDate,
                    e.ClassID,
                    e.IdSession,
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
                    Semester = param.Semester,
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

                var schedules = allSchedules.Where(x => homeroomIds.Contains(x.IdHomeroom)).ToList();

                item.AbsentTerm = mappingLevel.AbsentTerms;
                item.IsNeedValidation = mappingLevel.IsNeedValidation;
                item.TotalStudent = schedules.GroupBy(x => x.IdStudent).Count();

                item.Submitted = mappingLevel.AbsentTerms == AbsentTerm.Session
                    ? schedules
                        .Where(x => x.Items.Any(y => y == AttendanceEntryStatus.Submitted))
                        .GroupBy(x => new { x.ScheduleDate, x.ClassID, x.IdSession }).Count()
                    : schedules.Where(x => x.Items.Any(y => y == AttendanceEntryStatus.Submitted))
                        .GroupBy(x => new { x.ScheduleDate, x.IdHomeroom }).Count();
                item.Pending = mappingLevel.AbsentTerms == AbsentTerm.Session
                    ? schedules.Where(x => x.Items.Any(y => y == AttendanceEntryStatus.Pending))
                        .GroupBy(x => new { x.ScheduleDate, x.ClassID, x.IdSession })
                        .Count()
                    : schedules.Where(x => x.Items.Any(y => y == AttendanceEntryStatus.Pending))
                        .GroupBy(x => new { x.ScheduleDate, x.IdHomeroom })
                        .Count();
                item.Unsubmitted = mappingLevel.AbsentTerms == AbsentTerm.Session
                    ? schedules.Where(x => !x.Items.Any()
                                           && x.ScheduleDate <= _dateTime.ServerTime)
                        .GroupBy(x => new { x.ScheduleDate, x.ClassID, x.IdSession })
                        .Count()
                    : schedules.Where(x => !x.Items.Any()
                                           && x.ScheduleDate <= _dateTime.ServerTime)
                        .GroupBy(x => new { x.ScheduleDate, x.IdHomeroom })
                        .Count();
            }

            sw.Stop();

            Logger.LogInformation("period 2 takes {Seconds}", Math.Round(sw.Elapsed.TotalSeconds, 2));

            return Request.CreateApiResult2(result as object);
        }
    }
}
