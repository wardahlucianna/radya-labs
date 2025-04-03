using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryUnsubmittedByRangeHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetSummaryUnsubmittedByRangeHandler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSummaryUnsubmittedByRangeRequest>(nameof(GetSummaryUnsubmittedByRangeRequest.IdAcademicYear),
                                                                         nameof(GetSummaryUnsubmittedByPeriodRequest.IdLevel),
                                                                         nameof(GetSummaryUnsubmittedByRangeRequest.StartDate),
                                                                         nameof(GetSummaryUnsubmittedByRangeRequest.EndDate),
                                                                         nameof(GetSummaryUnsubmittedByRangeRequest.IdSchool),
                                                                         nameof(GetSummaryUnsubmittedByRangeRequest.IdUser));

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

            var homerooms = await _dbContext.Entity<MsHomeroom>()
                                            .Include(x => x.Grade).ThenInclude(x => x.Level)
                                            .Where(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                                            .Where(x => x.Grade.IdLevel == param.IdLevel)
                                            .Where(predicateHomeroom)
                                            .Where(predicateLevelPrincipalAndVicePrincipal)
                                            .Select(x => new
                                            {
                                                x.Id,
                                                x.Grade.IdLevel
                                            })
                                            .ToListAsync(CancellationToken);

            var idHomerooms = homerooms.Select(x => x.Id).Distinct().ToList();
            IReadOnlyList<IItemValueVm> items;
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
            var predicateSearch = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => 1 == 1);
            if (!string.IsNullOrEmpty(param.Search))
            {
                predicateSearch = predicateSearch.And(x =>
                x.HomeroomName.Contains(param.Search)
                || x.ClassID.Contains(param.Search)
                || x.TeacherName.Contains(param.Search)
                || x.SubjectName.Contains(param.Search)
                || x.SessionID.Contains(param.Search));
            }

            var predicateStudentStatus = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => 1 == 1);

            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new {x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus})
                .Where(x => (x.StartDate == param.StartDate.Date || x.EndDate == param.EndDate.Date 
                    || (x.StartDate < param.StartDate.Date
                        ? x.EndDate != null ? (x.EndDate > param.StartDate.Date && x.EndDate < param.EndDate.Date) || x.EndDate > param.EndDate.Date : x.StartDate <= param.StartDate.Date
                        : x.EndDate != null ? ((param.EndDate.Date > x.StartDate && param.EndDate.Date < x.EndDate) || param.EndDate.Date > x.EndDate) : x.StartDate <= param.StartDate.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
                .ToListAsync();

                List<string> studentId = new List<string>();

                List<DateTime> studentDateNonActive = new List<DateTime>();

                foreach(var studentStatus in checkStudentStatus)
                {
                    if(studentStatus.EndDate != null)
                    {
                        if(param.EndDate <= studentStatus.EndDate)
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

            // var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new {x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus})
            //     .Where(x => (x.StartDate == param.StartDate.Date || x.EndDate == param.EndDate.Date 
            //         || (x.StartDate < param.StartDate.Date
            //             ? (x.EndDate > param.StartDate.Date && x.EndDate < param.EndDate.Date) || x.EndDate > param.EndDate.Date
            //             : (param.EndDate.Date > x.StartDate && param.EndDate.Date < x.EndDate) || param.EndDate.Date > x.EndDate)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
            //     .ToListAsync();

            if(checkStudentStatus != null)
            {
                if(studentId.Any())
                    predicateStudentStatus = predicateStudentStatus.And(x => !studentId.ToList().Contains(x.GeneratedScheduleStudent.IdStudent));
            }
            var queries = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                           .Include(x => x.GeneratedScheduleStudent)
                                            .ThenInclude(x => x.Student)
                                                .ThenInclude(x => x.StudentGrades)
                                            .Include(x => x.Homeroom.Grade)
                                           .Where(x =>
                                           x.IsGenerated
                                                && x.ScheduleDate >= param.StartDate
                                                && x.ScheduleDate <= param.EndDate
                                                //&& x.ScheduleDate <= _dateTime.ServerTime
                                                && idHomerooms.Contains(x.IdHomeroom)
                                                )
                                           .Where(predicateLesson)
                                           .Where(predicateStudentGrade)
                                           .Include(x => x.AttendanceEntries)
                                           //.Where(x => !x.AttendanceEntries.Any())
                                           .Where(predicateStudentStatus)
                                           .Where(predicateSearch)
                                           .ToListAsync(CancellationToken);

            var schedulessubmittedList = queries.Where(x => x.AttendanceEntries.Count != 0).ToList();
            var schedulesUnsubmittedList = queries.Where(x => x.AttendanceEntries.Count == 0).ToList();
            var schedulesUnsubmitted = new List<TrGeneratedScheduleLesson>();
            foreach (var dataUnsubmitted in schedulesUnsubmittedList)
            {
                if (schedulessubmittedList.Any(x => x.ScheduleDate == dataUnsubmitted.ScheduleDate & x.ClassID == dataUnsubmitted.ClassID && x.IdSubject == dataUnsubmitted.IdSubject
                && x.SubjectName == dataUnsubmitted.SubjectName && x.IdHomeroom == dataUnsubmitted.IdHomeroom && x.HomeroomName == dataUnsubmitted.HomeroomName 
                && x.IdSession == dataUnsubmitted.IdSession 
                && x.SessionID == dataUnsubmitted.SessionID) == false)
                {
                    schedulesUnsubmitted.Add(dataUnsubmitted);
                }

            }

            var queryGroup = schedulesUnsubmitted.GroupBy(x => new { x.ScheduleDate, x.ClassID, x.IdSubject, x.SubjectName, x.IdHomeroom, x.HomeroomName, x.IdSession, x.SessionID }).ToList();

            var query = queryGroup.Select(x => new
            {
                x.Key.ScheduleDate,
                x.Key.ClassID,
                x.Key.SubjectName,
                x.Key.IdSubject,
                x.Key.HomeroomName,
                x.Key.IdHomeroom,
                x.Key.SessionID,
                x.Key.IdSession,
                TeacherName = string.Join(",", x.Select(x => x.TeacherName).Distinct()),
                IdUser = string.Join(",", x.Select(x => x.IdUser).Distinct()),
                Total = (x.Count() / x.Select(x => x.TeacherName).Distinct().Count())
            }).AsQueryable();

            if (param.OrderType == OrderType.Asc)
            {
                switch (param.OrderBy)
                {
                    case "date":
                        query = query.OrderBy(x => x.ScheduleDate);
                        break;
                    case "classId":
                        query = query.OrderBy(x => x.ClassID);
                        break;
                    case "teacher":
                        query = query.OrderBy(x => x.TeacherName);
                        break;
                    case "subject":
                        query = query.OrderBy(x => x.ScheduleDate);
                        break;
                    case "homeroom":
                        query = query.OrderBy(x => x.HomeroomName);
                        break;
                    case "session":
                        query = query.OrderBy(x => x.SessionID);
                        break;
                    default:
                        query = query.OrderBy(x => x.ScheduleDate);
                        break;
                };
            }
            else
            {
                switch (param.OrderBy)
                {
                    case "date":
                        query = query.OrderByDescending(x => x.ScheduleDate);
                        break;
                    case "classId":
                        query = query.OrderByDescending(x => x.ClassID);
                        break;
                    case "teacher":
                        query = query.OrderByDescending(x => x.TeacherName);
                        break;
                    case "subject":
                        query = query.OrderByDescending(x => x.SubjectName);
                        break;
                    case "homeroom":
                        query = query.OrderByDescending(x => x.HomeroomName);
                        break;
                    case "session":
                        query = query.OrderByDescending(x => x.SessionID);
                        break;
                    default:
                        query = query.OrderByDescending(x => x.ScheduleDate);
                        break;
                };
            }

            var columns = new[] { "date", "classId", "teacher", "subject", "homeroom", "session" };

            var queryData = query.AsNoTracking().AsEnumerable().SetPagination(param)
               .Select(x => new UnresolvedAttendanceGroupResult
               {
                   Date = x.ScheduleDate,
                   ClassID = x.ClassID,
                   Teacher = new ItemValueVm
                   {
                       Id = x.IdUser,
                       Description = x.TeacherName
                   },
                   Subject = new ItemValueVm
                   {
                       Id = x.IdSubject,
                       Description = x.SubjectName
                   },
                   Homeroom = new ItemValueVm
                   {
                       Id = x.IdHomeroom,
                       Description = x.HomeroomName
                   },
                   Session = new ItemValueVm
                   {
                       Id = x.IdSession,
                       Description = x.SessionID
                   },
                   TotalStudent = x.Total
               }).ToList();

            var count = param.CanCountWithoutFetchDb(queryData.Count)
            ? queryData.Count
            : query.Select(x => x.ScheduleDate).Count();

            return Request.CreateApiResult2(queryData as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
