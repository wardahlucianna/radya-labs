using System;
using System.Collections.Generic;
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
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryDetailBySchoolDayByPeriodHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetSummaryDetailBySchoolDayByPeriodHandler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();

            var param = Request.ValidateParams<GetSummaryDetailByPeriodRequest>(nameof(GetSummaryDetailByPeriodRequest.IdAcademicYear),
                                                                               nameof(GetSummaryDetailByPeriodRequest.IdLevel),
                                                                               nameof(GetSummaryDetailByPeriodRequest.Semester));
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
            var predicateLesson = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => 1 == 1);
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
                        predicateLesson = predicateLesson.And(x => IdSubject.Contains(x.IdSubject));
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
                        predicateHomeroom = PredicateBuilder.Create<MsHomeroom>(x => dataHomeroomTeacher.Contains(x.Id) && x.HomeroomTeachers.Any(ht => ht.IdBinusian == param.IdUser));
                }

                if (param.SelectedPosition == PositionConstant.SubjectTeacher)
                {
                    if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
                    {
                        predicateLesson = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => dataLessonTeacher.Contains(x.IdLesson) && x.IdUser == param.IdUser);
                        predicateHomeroom = predicateHomeroom.And(x => x.HomeroomPathways.Any(y => y.LessonPathways.Any(z => dataLessonTeacher.Contains(z.IdLesson))));
                    }

                }
            }
            #endregion

            if (!string.IsNullOrEmpty(param.ClassId))
                predicateLesson = predicateLesson.And(x => x.ClassID == param.ClassId);
            if (!string.IsNullOrEmpty(param.IdSession))
                predicateLesson = predicateLesson.And(x => x.IdSession == param.IdSession);
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
                predicateLesson = predicateLesson.And(x => allowIsAttendanceEntryByClassId.Contains(x.IdLesson));
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
            var schedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                            .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                            .Include(x => x.GeneratedScheduleStudent).ThenInclude(x => x.Student)
                                            .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceEntryWorkhabits).ThenInclude(x => x.MappingAttendanceWorkhabit)
                                            .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                                            .Where(x => x.IsGenerated
                                                        && homeroomIds.Contains(x.IdHomeroom))
                                            .Where(predicateLesson)
                                            .Where(predicateStudentGrade)
                                            .ToListAsync(CancellationToken);

            var result = await _dbContext.Entity<MsLevel>()
                                         .Include(x => x.AcademicYear)
                                         .Where(x => x.Id == param.IdLevel
                                                     && x.IdAcademicYear == param.IdAcademicYear)
                                        .Where(predicateLevel)
                                        .Where(x => x.Id == param.IdLevel)
                                         .Select(x => new GetSummaryDetailResult<SummaryBySchoolDayResult>
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
                                         }).SingleOrDefaultAsync(CancellationToken);
            if (result is null)
            {
                var dataGrade = await _dbContext.Entity<MsGrade>().Where(x => x.Id == param.IdGrade).FirstOrDefaultAsync(CancellationToken);
                var dataUser = await _dbContext.Entity<MsStaff>().Where(x => x.IdBinusian == param.IdUser).FirstOrDefaultAsync(CancellationToken);
                throw new NotFoundException($"{dataUser.FirstName} {dataUser.LastName} has no assign to {dataGrade.Description}");
            }


            var mapping = await _dbContext.Entity<MsMappingAttendance>()
                                          .Include(x => x.AttendanceMappingAttendances).ThenInclude(x => x.Attendance)
                                          .Include(x => x.MappingAttendanceWorkhabits).ThenInclude(x => x.Workhabit)
                                          .Where(x => x.IdLevel == result.Level.Id)
                                          .FirstOrDefaultAsync(CancellationToken);
            if (mapping is null)
                throw new NotFoundException("mapping is not found for this level");

            var eaMapping = await _dbContext.Entity<MsSchoolMappingEA>()
                                            .Include(x => x.School).ThenInclude(x => x.AcademicYears).ThenInclude(x => x.Levels)
                                            .Where(x => x.School.AcademicYears.Any(y => y.Levels.Any(z => z.Id == param.IdLevel)))
                                            .FirstOrDefaultAsync(CancellationToken);
            if (eaMapping is null)
                throw new NotFoundException("EA mapping is not found for this school");

            result.IsEAGrouped = eaMapping.IsGrouped;
            result.IsUseDueToLateness = mapping.IsUseDueToLateness;
            result.Term = mapping.AbsentTerms;

            result.Data = schedules.GroupBy(x => new { x.DaysOfWeek, x.ScheduleDate.Date, x.IdHomeroom, x.HomeroomName }).Select(x => new SummaryBySchoolDayResult
            {
                Homeroom = new ItemValueVm
                {
                    Id = x.Key.IdHomeroom,
                    Description = x.Key.HomeroomName
                },
                DayOfWeek = x.Key.DaysOfWeek,
                Date = x.Key.Date,
                Sessions = GetSessions(mapping, x)
            }).ToList();

            //get last updated data
            if (schedules.Any(x => x.AttendanceEntries.Any()))
            {
                var lastCreated = schedules.Max(x => x.AttendanceEntries.Max(y => y.DateIn));
                var lastUpdated = schedules.Max(x => x.AttendanceEntries.Max(y => y.DateUp));
                if (lastCreated.HasValue && lastUpdated.HasValue)
                    result.LastUpdated = lastCreated > lastUpdated ? lastCreated : lastUpdated;
                else if (lastCreated.HasValue)
                    result.LastUpdated = lastCreated;
                else if (lastUpdated.HasValue)
                    result.LastUpdated = lastUpdated;
            }

            return Request.CreateApiResult2(result as object);
        }

        private List<Session> GetSessions(MsMappingAttendance mapping, IEnumerable<TrGeneratedScheduleLesson> data)
        {
            var result = data.GroupBy(x => new { x.IdSession, x.SessionID, x.ClassID })
                             .Select(x => new Session
                             {
                                 Id = x.Key.IdSession,
                                 Description = x.Key.SessionID,
                                 ClassId = x.Key.ClassID,
                                 Attendances = mapping.AttendanceMappingAttendances.Select(y => new AttendanceData
                                 {
                                     Id = y.Id,
                                     Code = y.Attendance.Code,
                                     Description = y.Attendance.Description,
                                     Students = x.Where(z => z.AttendanceEntries.Any(a => a.AttendanceMappingAttendance.IdAttendance == y.IdAttendance && a.Status == AttendanceEntryStatus.Submitted))
                                                 .Select(z => new ItemValueVm
                                                 {
                                                     Id = z.GeneratedScheduleStudent.Student.Id,
                                                     Description = $"{z.GeneratedScheduleStudent.Student.FirstName} {z.GeneratedScheduleStudent.Student.LastName}"
                                                 }).ToList()
                                 }).ToList(),
                                 Workhabits = mapping.MappingAttendanceWorkhabits.Select(y => new AttendanceData
                                 {
                                     Id = y.Id,
                                     Code = y.Workhabit.Code,
                                     Description = y.Workhabit.Description,
                                     Students = x.Where(z => z.AttendanceEntries.Any(a => a.AttendanceEntryWorkhabits.Any(b => b.MappingAttendanceWorkhabit.Id == y.Id) && a.Status == AttendanceEntryStatus.Submitted))
                                                 .Select(z => new ItemValueVm
                                                 {
                                                     Id = z.GeneratedScheduleStudent.Student.Id,
                                                     Description = $"{z.GeneratedScheduleStudent.Student.FirstName} {z.GeneratedScheduleStudent.Student.LastName}"
                                                 }).ToList()
                                 }).ToList(),
                                 Unsubmitted = x.Where(y => !y.AttendanceEntries.Any()
                                                            && y.ScheduleDate.Date <= _dateTime.ServerTime.Date)
                                                .Select(y => new ItemValueVm
                                                {
                                                    Id = y.GeneratedScheduleStudent.Student.Id,
                                                    Description = $"{y.GeneratedScheduleStudent.Student.FirstName} {y.GeneratedScheduleStudent.Student.LastName}"
                                                }).ToList(),
                                 Pending = x.Where(y => y.AttendanceEntries.Any(z => z.Status == AttendanceEntryStatus.Pending))
                                            .Select(y => new ItemValueVm
                                            {
                                                Id = y.GeneratedScheduleStudent.Student.Id,
                                                Description = $"{y.GeneratedScheduleStudent.Student.FirstName} {y.GeneratedScheduleStudent.Student.LastName}"
                                            }).ToList()
                             })
                             .ToList();
            return mapping.AbsentTerms == AbsentTerm.Day
                ? new List<Session>
                {
                    result.OrderBy(x=> x.Description).First()
                }
                : result;
        }
    }
}
