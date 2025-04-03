using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetAttendanceRateByStudentTermDayHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        public GetAttendanceRateByStudentTermDayHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();

            var param = Request.ValidateParams<GetAttendanceRateByStudentRequest>(nameof(GetAttendanceRateByStudentRequest.IdStudent),
                                                                                  //nameof(GetAttendanceRateByStudentRequest.IdHomeroom),
                                                                                  nameof(GetAttendanceRateByStudentRequest.IdUser),
                                                                                  nameof(GetAttendanceRateByStudentRequest.IdAcademicYear));
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
            //if (positionUser.Count == 0 && dataHomeroomTeacher.Count == 0 && dataLessonTeacher.Count == 0)
            //    throw new BadRequestException($"You dont have any position.");
            if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
                avaiablePosition.Add(PositionConstant.ClassAdvisor);
            if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
                avaiablePosition.Add(PositionConstant.SubjectTeacher);
            foreach (var pu in positionUser)
            {
                avaiablePosition.Add(pu.Code);
            }
            //if (avaiablePosition.Where(x => x == param.SelectedPosition).Count() == 0)
            //    throw new BadRequestException($"You dont assign as {param.SelectedPosition}.");
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

            var schedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                            .Include(x => x.GeneratedScheduleStudent)
                                            .Include(x => x.AttendanceEntries).ThenInclude(x => x.AttendanceMappingAttendance).ThenInclude(x => x.Attendance)
                                            .Where(x => x.IsGenerated
                                                        && x.GeneratedScheduleStudent.IdStudent == param.IdStudent
                                                        //&& x.IdHomeroom == param.IdHomeroom
                                                        && (!param.StartDate.HasValue || x.ScheduleDate.Date >= param.StartDate)
                                                        && (!param.EndDate.HasValue || x.ScheduleDate.Date <= param.EndDate))
                                            .Where(predicateLesson)
                                            .ToListAsync();

            //var isUseDueToLateness = await _dbContext.Entity<MsMappingAttendance>()
            //                                         .Include(x => x.Level)
            //                                            .ThenInclude(x => x.Grades)
            //                                                .ThenInclude(x => x.Homerooms)
            //                                         .Where(x => x.Level.Grades.Any(y => y.Homerooms.Any(z => z.Id == param.IdHomeroom)))
            //                                         .Select(x => x.IsUseDueToLateness)
            //                                         .FirstOrDefaultAsync(CancellationToken);

            var result = new GetAttendanceRateByStudentTermDayResult
            {
                TotalDay = schedules.GroupBy(x => x.ScheduleDate).Count(),
                Present = schedules.Where(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Present && (z.AttendanceMappingAttendance.Attendance.Code != "LT" && z.AttendanceMappingAttendance.Attendance.Code != "LTA") && z.AttendanceMappingAttendance.Attendance.AbsenceCategory == null && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count(),
                ExcusedAbsent = schedules.Where(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Excused && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count(),
                //UnexcusedAbsent = isUseDueToLateness
                //                  ? schedules.Where(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count() +
                //                    Math.Abs(schedules.Where(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.Code == "LT" && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count() / 4)
                //                  : schedules.Where(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count(),
                UnexcusedAbsent = schedules.Where(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count(),
                Late = schedules.Where(y => y.AttendanceEntries.Any(z => (z.AttendanceMappingAttendance.Attendance.Code == "LT" || z.AttendanceMappingAttendance.Attendance.Code == "LTA") && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count(),
                //PresenceRate = isUseDueToLateness
                //               ? (double)(schedules.GroupBy(x => x.ScheduleDate).Count() - schedules.Where(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Absent && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count()
                //                 - Math.Abs(schedules.Where(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.Code == "LT" && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count() / 4)) / schedules.GroupBy(x => x.ScheduleDate).Count()
                //               : (double)(schedules.GroupBy(x => x.ScheduleDate).Count() - schedules.Where(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Absent && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count()) / schedules.GroupBy(x => x.ScheduleDate).Count(),
                PresenceRate = (double)(schedules.GroupBy(x => x.ScheduleDate).Count() - schedules.Where(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Absent && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count()) / schedules.GroupBy(x => x.ScheduleDate).Count(),
                PunctualityRate = (double)(schedules.GroupBy(x => x.ScheduleDate).Count() - (schedules.Where(y => y.AttendanceEntries.Any(z => (z.AttendanceMappingAttendance.Attendance.Code == "LT" || z.AttendanceMappingAttendance.Attendance.Code == "LTA") && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count() +
                                                                    schedules.Where(y => y.AttendanceEntries.Any(z => z.AttendanceMappingAttendance.Attendance.AttendanceCategory == AttendanceCategory.Absent && z.Status == AttendanceEntryStatus.Submitted)).GroupBy(x => x.ScheduleDate).Count())) / schedules.GroupBy(x => x.ScheduleDate).Count(),
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
