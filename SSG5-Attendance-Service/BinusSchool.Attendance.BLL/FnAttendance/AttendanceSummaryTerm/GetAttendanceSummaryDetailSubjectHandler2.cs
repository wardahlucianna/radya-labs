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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Models;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailSubjectHandler2 : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceSummaryService _attendanceSummaryService;
        private readonly IAttendanceSummaryRedisService _attendanceSummaryRedisService;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<GetAttendanceSummaryDetailSubjectHandler2> _logger;

        public GetAttendanceSummaryDetailSubjectHandler2(
            IAttendanceSummaryService attendanceSummaryService,
            IAttendanceSummaryRedisService attendanceSummaryRedisService,
            IAttendanceDbContext dbContext,
            ILogger<GetAttendanceSummaryDetailSubjectHandler2> logger)
        {
            _attendanceSummaryService = attendanceSummaryService;
            _attendanceSummaryRedisService = attendanceSummaryRedisService;
            _dbContext = dbContext;
            _logger = logger;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                return await ExecuteAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurs");
                throw;
            }
        }

        private async Task<ApiErrorResult<object>> ExecuteAsync()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryDetailSubjectRequest>();
            var columns = new[] { "homeroom", "classIdSubject", "teacherName", "unsubmited", "pending" };

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
                .FirstOrDefaultAsync(CancellationToken);
            if (mappingAttendance is null)
                throw new Exception("Data mapping is missing");

            var filterIdHomerooms = new GetHomeroomByIdUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                SelectedPosition = param.SelectedPosition,
                IdUser = param.IdUser,
                IdLevel = param.IdLevel,
                IdGrade = param.IdGrade,
                IdSubject = param.IdSubject
            };

            var listIdLesson =
                await _attendanceSummaryRedisService.GetLessonByUser(filterIdHomerooms, CancellationToken);

            var scheduleLessonAsQueryable = (await _attendanceSummaryRedisService.GetScheduleLessonAsync(
                    param.IdAcademicYear,
                    param.IdLevel,
                    CancellationToken))
                .Where(e => e.ScheduleDate.Date >= param.StartDate.Date &&
                            e.ScheduleDate.Date <= param.EndDate.Date)
                .AsQueryable();

            if (!string.IsNullOrEmpty(param.IdGrade))
                scheduleLessonAsQueryable = scheduleLessonAsQueryable.Where(e => e.IdGrade == param.IdGrade);

            if (!string.IsNullOrEmpty(param.IdSubject))
                scheduleLessonAsQueryable =
                    scheduleLessonAsQueryable.Where(e => e.Subject.Id == param.IdSubject);

            if (listIdLesson.Any())
                scheduleLessonAsQueryable = scheduleLessonAsQueryable.Where(e => listIdLesson.Contains(e.IdLesson));

            var scheduleLessons = scheduleLessonAsQueryable
                .GroupBy(e => new
                {
                    e.ClassID,
                    e.Subject.Description
                })
                .ToDictionary(e => e.Key, y => y.ToList());

            if (!scheduleLessons.Any())
                return Request.CreateApiResult2(new List<GetAttendanceSummaryDetailSubjectResult>() as object,
                    param.CreatePaginationProperty(0).AddColumnProperty(columns));

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
                throw new Exception("Invalid grade");

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

            var groupedAttendanceEntries = await _attendanceSummaryService.GetAttendanceEntriesGroupedAsync(
                scheduleLessons.SelectMany(e => e.Value)
                    .Select(e => e.Id)
                    .ToArray(), CancellationToken);

            var querySchedule = _dbContext.Entity<MsSchedule>()
                .Include(e => e.User)
                .Include(e => e.Lesson)
                .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.IdLevel))
                querySchedule = querySchedule.Where(e => e.Lesson.Grade.IdLevel == param.IdLevel);

            var listSchedule = await querySchedule
                .GroupBy(e => new RedisAttendanceSummaryScheduleResult
                {
                    IdLesson = e.IdLesson,
                    Teacher = new RedisAttendanceSummaryTeacher
                    {
                        IdUser = e.IdUser,
                        FirstName = e.User.FirstName,
                        LastName = e.User.LastName
                    },
                    IdWeek = e.IdWeek,
                    IdDay = e.IdDay,
                    IdSession = e.IdSession,
                })
                .Select(e => e.Key)
                .ToListAsync(CancellationToken);

            var dataSubject = new List<GetAttendanceSummaryDetailSubjectResult>();

            //start logic
            foreach (var item in scheduleLessons)
            {
                if (!item.Value.Any())
                    continue;

                var first = item.Value.First();

                var teacher = listSchedule
                    .FirstOrDefault(e => e.IdLesson == first.IdLesson &&
                                         e.IdSession == first.Session.Id &&
                                         e.IdWeek == first.IdWeek &&
                                         e.IdDay == first.IdDay);

                foreach (var item2 in first.LessonPathwayResults)
                {
                    foreach (var homeroom in listMappingSemesterGradeIdClassroomAndIdHomeroom)
                    {
                        if (!(homeroom.Semester == first.Semester &&
                              homeroom.IdGrade == first.IdGrade &&
                              item2.IdHomeroom == homeroom.IdHomeroom))
                            continue;

                        var vm = new GetAttendanceSummaryDetailSubjectResult
                        {
                            IdAcademicYear = first.IdAcademicYear,
                            IdLevel = first.IdLevel,
                            IdGrade = first.IdGrade,
                            IdSubject = first.Subject.Id,
                            ClassIdSubject = $"{item.Key.ClassID}-{item.Key.Description}",
                            TeacherName = teacher != null
                                ? NameUtil.GenerateFullName(teacher.Teacher.FirstName, teacher.Teacher.LastName)
                                : string.Empty,
                            Homeroom = homeroom.GradeCode + homeroom.ClassroomCode,
                            IdHomeroom = homeroom.IdHomeroom,
                            //Unsubmited = countUnsubmited,
                            //Pending = countPending,
                        };

                        var listStudentEnrolled = dict[(homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)];

                        var groupedByDate = item.Value.GroupBy(e => e.ScheduleDate)
                            .ToDictionary(e => e.Key, e => e.ToList());

                        int index = 0;
                        foreach (var item4 in groupedByDate)
                        {
                            foreach (var item5 in item4.Value)
                            {
                                if (index > 0 && mappingAttendance.AbsentTerms == AbsentTerm.Day)
                                    break;

                                if (!groupedAttendanceEntries.ContainsKey(item5.Id))
                                {
                                    //loop every student enrolled
                                    foreach (var studentEnrolled in listStudentEnrolled)
                                    {
                                        //logic student status
                                        var studentStatus = studentStatuses
                                            .FirstOrDefault(e =>
                                                e.IdStudent == studentEnrolled.IdStudent &&
                                                e.StartDt.Date <= item5.ScheduleDate.Date &&
                                                item5.ScheduleDate.Date <= e.EndDt.Value.Date);
                                        //student status is null or not active, skipped
                                        if (studentStatus is null || studentStatus.IsActive == false)
                                            continue;

                                        var passed = false;
                                        //logic moving
                                        foreach (var current in studentEnrolled.Items)
                                        {
                                            if (passed)
                                                break;

                                            if (string.IsNullOrWhiteSpace(current.IdLesson))
                                                continue;

                                            if (current.Ignored || current.IdLesson != item5.IdLesson)
                                                continue;

                                            //when current id lesson is same as above and the date of the current moving still satisfied
                                            //then set to passed, other than that will be excluded
                                            if (current.StartDt.Date <= item5.ScheduleDate.Date &&
                                                item5.ScheduleDate.Date < current.EndDt.Date)
                                                passed = true;
                                        }

                                        if (!passed)
                                            continue;

                                        vm.Unsubmited++;
                                    }

                                    index++;
                                    continue;
                                }

                                var attendanceEntries = groupedAttendanceEntries[item5.Id];

                                //loop every student enrolled
                                foreach (var studentEnrolled in listStudentEnrolled)
                                {
                                    //logic student status
                                    var studentStatus = studentStatuses
                                        .FirstOrDefault(e =>
                                            e.IdStudent == studentEnrolled.IdStudent &&
                                            e.StartDt.Date <= item5.ScheduleDate.Date &&
                                            item5.ScheduleDate.Date <= e.EndDt.Value.Date);
                                    //student status is null or not active, skipped
                                    if (studentStatus is null || studentStatus.IsActive == false)
                                        continue;

                                    var passed = false;
                                    //logic moving
                                    foreach (var current in studentEnrolled.Items)
                                    {
                                        if (passed)
                                            break;

                                        if (string.IsNullOrWhiteSpace(current.IdLesson))
                                            continue;

                                        if (current.Ignored || current.IdLesson != item5.IdLesson)
                                            continue;

                                        //when current id lesson is same as above and the date of the current moving still satisfied
                                        //then set to passed, other than that will be excluded
                                        if (current.StartDt.Date <= item5.ScheduleDate.Date &&
                                            item5.ScheduleDate.Date < current.EndDt.Date)
                                            passed = true;
                                    }

                                    if (!passed)
                                        continue;

                                    var attendanceEntry = attendanceEntries.Where(e =>
                                            e.IdHomeroomStudent == studentEnrolled.IdHomeroomStudent)
                                        .OrderByDescending(e => e.DateIn)
                                        .FirstOrDefault();

                                    if (attendanceEntry is null)
                                        vm.Unsubmited++;
                                    else
                                    {
                                        if (attendanceEntry.Status == AttendanceEntryStatus.Pending)
                                            vm.Pending++;
                                    }
                                }

                                index++;
                            }
                        }

                        dataSubject.Add(vm);
                    }
                }
            }
            //end logic

            var querySubject = dataSubject.Distinct();

            if (!string.IsNullOrEmpty(param.Search))
                querySubject = querySubject.Where(e =>
                    e.ClassIdSubject.ToLower().Contains(param.Search.ToLower()) ||
                    e.TeacherName.ToLower().Contains(param.Search.ToLower()));

            switch (param.OrderBy)
            {
                case "homeroom":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.Homeroom)
                        : querySubject.OrderBy(x => x.Homeroom);
                    break;
                case "classIdSubject":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.ClassIdSubject)
                        : querySubject.OrderBy(x => x.ClassIdSubject);
                    break;
                case "teacherName":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.TeacherName)
                        : querySubject.OrderBy(x => x.TeacherName);
                    break;
                case "unsubmited":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.Unsubmited)
                        : querySubject.OrderBy(x => x.Unsubmited);
                    break;
                case "pending":
                    querySubject = param.OrderType == OrderType.Desc
                        ? querySubject.OrderByDescending(x => x.Pending)
                        : querySubject.OrderBy(x => x.Pending);
                    break;
            }

            IReadOnlyList<IItemValueVm> items;
            var getAttendanceSummaryDetailSubjectResults = querySubject.ToList();
            if (param.Return == CollectionType.Lov)
            {
                items = getAttendanceSummaryDetailSubjectResults;
            }
            else
            {
                items = getAttendanceSummaryDetailSubjectResults
                    .SetPagination(param)
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : getAttendanceSummaryDetailSubjectResults.Select(x => x.IdSubject).Count();

            return Request.CreateApiResult2((object)items,
                param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
