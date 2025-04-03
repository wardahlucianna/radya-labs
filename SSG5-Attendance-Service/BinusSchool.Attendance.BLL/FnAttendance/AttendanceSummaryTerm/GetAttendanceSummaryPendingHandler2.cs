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
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryPendingHandler2 : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceSummaryRedisService _attendanceSummaryRedisService;
        private readonly IAttendanceSummaryService _attendanceSummaryService;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceSummary _apiAttendanceSummary;
        private readonly ILogger<GetAttendanceSummaryDetailSchoolDayHandler2> _logger;

        public GetAttendanceSummaryPendingHandler2(IAttendanceSummaryRedisService attendanceSummaryRedisService,
            IAttendanceSummaryService attendanceSummaryService,
            IAttendanceDbContext dbContext,
            IAttendanceSummary apiAttendanceSummary,
            ILogger<GetAttendanceSummaryDetailSchoolDayHandler2> logger)
        {
            _attendanceSummaryRedisService = attendanceSummaryRedisService;
            _attendanceSummaryService = attendanceSummaryService;
            _dbContext = dbContext;
            _apiAttendanceSummary = apiAttendanceSummary;
            _logger = logger;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceSummaryPendingRequest>();
            string[] _columns = { "date", "clasId", "teacher", "homeroom", "subjectId", "sessionId", "totalstudent" };

            var filterIdHomerooms = new GetHomeroomByIdUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                SelectedPosition = param.SelectedPosition,
                IdUser = param.IdUser,
                IdLevel = param.IdLevel,
                IdGrade = param.IdGrade,
                IdHomeroom = param.IdHomeroom,
                IdSubject = param.IdSubject,
                IdClassroom = param.IdClassroom
            };

            var listIdLesson = await _attendanceSummaryRedisService.GetLessonByUser(filterIdHomerooms, CancellationToken);

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

            var scheduleLessonAsQueryable = (await _attendanceSummaryRedisService.GetScheduleLessonAsync(
                    param.IdAcademicYear,
                    param.IdLevel, CancellationToken))
                .AsQueryable();

            if (param.StartDate.HasValue)
                scheduleLessonAsQueryable = scheduleLessonAsQueryable.Where(e =>
                    e.ScheduleDate.Date >= param.StartDate.Value.Date);

            if (param.EndDate.HasValue)
                scheduleLessonAsQueryable = scheduleLessonAsQueryable.Where(e =>
                    e.ScheduleDate.Date <= param.EndDate.Value.Date);

            if (!string.IsNullOrEmpty(param.IdSubject))
                scheduleLessonAsQueryable =
                    scheduleLessonAsQueryable.Where(e => e.Subject.Id == param.IdSubject);

            if (!string.IsNullOrEmpty(param.IdGrade))
                scheduleLessonAsQueryable = scheduleLessonAsQueryable.Where(e => e.IdGrade == param.IdGrade);

            if (listIdLesson.Any())
                scheduleLessonAsQueryable = scheduleLessonAsQueryable.Where(e => listIdLesson.Contains(e.IdLesson));

            var scheduleLessons = scheduleLessonAsQueryable.ToList();
            if (!scheduleLessons.Any())
                return Request.CreateApiResult2(new List<GetAttendanceSummaryPendingResult>() as object,
                    param.CreatePaginationProperty(0).AddColumnProperty(_columns));

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
                .Where(e => e.IdAcademicYear == param.IdAcademicYear && idGrades.Contains(e.IdGrade) && e.Grade.Level.Id == param.IdLevel)
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

            //homeroomTeachers
            var queryHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
                .Include(e => e.TeacherPosition).ThenInclude(e => e.LtPosition)
                .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Include(e => e.TeacherPosition).ThenInclude(e => e.LtPosition)
                .Include(e => e.Staff)
                .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.IdLevel))
                queryHomeroomTeacher = queryHomeroomTeacher.Where(e => idGrades.Contains(e.Homeroom.IdGrade));

            var listHomeroomTeacher = await queryHomeroomTeacher
                .GroupBy(e => new RedisAttendanceSummaryHomeroomTeacherResult
                {
                    IdHomeroom = e.Homeroom.Id,
                    IdGrade = e.Homeroom.IdGrade,
                    IdClassroom = e.Homeroom.GradePathwayClassroom.IdClassroom,
                    IsAttendance = e.IsAttendance,
                    Teacher = new RedisAttendanceSummaryTeacher
                    {
                        IdUser = e.IdBinusian,
                        FirstName = e.Staff.FirstName,
                        LastName = e.Staff.LastName,
                    },
                    Position = new CodeWithIdVm
                    {
                        Id = e.TeacherPosition.LtPosition.Id,
                        Code = e.TeacherPosition.LtPosition.Code,
                        Description = e.TeacherPosition.LtPosition.Description
                    }
                })
                .Select(e => e.Key)
                .ToListAsync(CancellationToken);

            var listIdStudent = dict.SelectMany(e => e.Value).Select(e => e.IdStudent).Distinct().ToArray();
            var studentStatuses =
                await _attendanceSummaryService.GetStudentStatusesAsync(listIdStudent, param.IdAcademicYear,
                    CancellationToken);

            var groupedAttendancePendingEntries = await _attendanceSummaryService.GetAttendanceEntriesPendingGroupedAsync(
                    scheduleLessons
                        .Select(e => e.Id)
                        .ToArray(), CancellationToken);

            scheduleLessons = scheduleLessons.Where(e => groupedAttendancePendingEntries.Select(x => x.Key).Contains(e.Id)).ToList();

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

            var list = new List<GetAttendanceSummaryPendingResult>();

            if (mappingAttendance.AbsentTerms == AbsentTerm.Session)
            {
                var finalScheduleLessons = scheduleLessons
                    .GroupBy(e => new
                    {
                        e.ClassID,
                        e.Subject.Description
                    })
                    .ToDictionary(e => e.Key, y => y.ToList());

                foreach (var scheduleLesson in finalScheduleLessons)
                {
                    foreach (var item2 in scheduleLesson.Value)
                    {
                        var teacher = listSchedule
                            .FirstOrDefault(e => e.IdLesson == item2.IdLesson &&
                                                 e.IdSession == item2.Session.Id &&
                                                 e.IdWeek == item2.IdWeek &&
                                                 e.IdDay == item2.IdDay);
                        if (teacher is null)
                            continue;

                        foreach (var homeroom in listMappingSemesterGradeIdClassroomAndIdHomeroom
                            .Where(x => x.Semester == item2.Semester && x.IdGrade == item2.IdGrade 
                            && item2.LessonPathwayResults.Any(e => e.IdHomeroom == x.IdHomeroom)))
                        {
                            if (!string.IsNullOrWhiteSpace(param.IdHomeroom) && param.IdHomeroom != homeroom.IdHomeroom)
                                continue;
                            if (!dict.ContainsKey((homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)))
                                continue;

                            var homeroomTeacher = listHomeroomTeacher
                            .FirstOrDefault(e => e.IdHomeroom == homeroom.IdHomeroom && e.IsAttendance);

                            var vm = new GetAttendanceSummaryPendingResult
                            {
                                Date = item2.ScheduleDate,
                                ClassID = item2.ClassID,
                                Teacher = new ItemValueVm
                                {
                                    Id = teacher?.Teacher.IdUser,
                                    Description = teacher is null
                                        ? null
                                        : NameUtil.GenerateFullName(teacher.Teacher.FirstName, teacher.Teacher.LastName)
                                },
                                HomeroomTeacher = homeroomTeacher is null
                                ? null
                                : new ItemValueVm
                                {
                                    Id = homeroomTeacher.Teacher.IdUser,
                                    Description = NameUtil.GenerateFullName(homeroomTeacher.Teacher.FirstName,
                                        homeroomTeacher.Teacher.LastName)
                                },
                                Homeroom = new ItemValueVm
                                {
                                    Id = homeroom.IdHomeroom,
                                    Description = homeroom.GradeCode + homeroom.ClassroomCode
                                },
                                SubjectId = item2.Subject.SubjectID,
                                Session = new RedisAttendanceSummarySession
                                {
                                    Id = item2.Session.Id,
                                    Name = item2.Session.Name,
                                    SessionID = item2.Session.SessionID
                                }
                            };

                            var listStudentEnrolled = dict[(homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)];

                            var attendanceEntries = groupedAttendancePendingEntries[item2.Id];

                            //loop every student enrolled
                            foreach (var studentEnrolled in listStudentEnrolled)
                            {
                                //logic student status
                                var studentStatus = studentStatuses
                                    .FirstOrDefault(e => e.IdStudent == studentEnrolled.IdStudent && e.IsActive);
                                //student status is null, skipped
                                if (studentStatus is null)
                                    continue;

                                var passed = false;
                                //logic moving
                                foreach (var current in studentEnrolled.Items)
                                {
                                    if (passed)
                                        break;

                                    if (string.IsNullOrWhiteSpace(current.IdLesson))
                                        continue;

                                    if (current.Ignored || current.IdLesson != item2.IdLesson)
                                        continue;

                                    //when current id lesson is same as above and the date of the current moving still satisfied
                                    //then set to passed, other than that will be excluded
                                    if (current.StartDt.Date <= item2.ScheduleDate.Date &&
                                        item2.ScheduleDate.Date <= current.EndDt.Date)
                                        passed = true;
                                }

                                if (!passed)
                                    continue;
                                var attendanceEntry = attendanceEntries.Where(e =>
                                        e.IdHomeroomStudent == studentEnrolled.IdHomeroomStudent)
                                    .OrderByDescending(e => e.DateIn)
                                    .FirstOrDefault();

                                if (attendanceEntry != null)
                                    vm.TotalStudent++;
                            }

                            if (vm.TotalStudent > 0)
                                list.Add(vm);
                        }
                    }
                }
            }
            else
            {
                var finalScheduleLessons = scheduleLessons
                    .GroupBy(e => new
                    {
                        e.ScheduleDate
                    })
                    .ToDictionary(e => e.Key, y => y.First());

                foreach (var scheduleLesson in finalScheduleLessons)
                {
                    var item2 = scheduleLesson.Value;

                    var teacher = listSchedule
                        .FirstOrDefault(e => e.IdLesson == item2.IdLesson &&
                                             e.IdSession == item2.Session.Id &&
                                             e.IdWeek == item2.IdWeek &&
                                             e.IdDay == item2.IdDay);
                    if (teacher is null)
                        continue;

                    foreach (var homeroom in listMappingSemesterGradeIdClassroomAndIdHomeroom.Where(x => x.Semester == item2.Semester
                    && x.IdGrade == item2.IdGrade
                    && item2.LessonPathwayResults.Any(e => e.IdHomeroom == x.IdHomeroom)))
                    {
                        var vm = new GetAttendanceSummaryPendingResult
                        {
                            Date = item2.ScheduleDate,
                            ClassID = item2.ClassID,
                            Teacher = new ItemValueVm
                            {
                                Id = teacher?.Teacher.IdUser,
                                Description = teacher is null
                                    ? null
                                    : NameUtil.GenerateFullName(teacher.Teacher.FirstName, teacher.Teacher.LastName)
                            },
                            Homeroom = new ItemValueVm
                            {
                                Id = homeroom.IdHomeroom,
                                Description = homeroom.GradeCode + homeroom.ClassroomCode
                            },
                            SubjectId = item2.Subject.SubjectID,
                            Session = new RedisAttendanceSummarySession
                            {
                                Id = item2.Session.Id,
                                Name = item2.Session.Name,
                                SessionID = item2.Session.SessionID
                            }
                        };

                        if (!dict.ContainsKey((homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)))
                            continue;

                        var listStudentEnrolled = dict[(homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)];

                        var attendanceEntries = groupedAttendancePendingEntries[item2.Id];

                        //loop every student enrolled
                        foreach (var studentEnrolled in listStudentEnrolled)
                        {
                            //logic student status when status is active
                            var studentStatus = studentStatuses
                                .FirstOrDefault(e => e.IdStudent == studentEnrolled.IdStudent && e.IsActive);
                            //student status is null, skipped
                            if (studentStatus is null)
                                continue;

                            var passed = false;
                            //logic moving
                            foreach (var current in studentEnrolled.Items)
                            {
                                if (passed)
                                    break;

                                if (string.IsNullOrWhiteSpace(current.IdLesson))
                                    continue;

                                if (current.Ignored || current.IdLesson != item2.IdLesson)
                                    continue;

                                //when current id lesson is same as above and the date of the current moving still satisfied
                                //then set to passed, other than that will be excluded
                                if (current.StartDt.Date <= item2.ScheduleDate.Date &&
                                    item2.ScheduleDate.Date <= current.EndDt.Date)
                                    passed = true;
                            }

                            if (!passed)
                                continue;
                            var attendanceEntry = attendanceEntries.Where(e =>
                                    e.IdHomeroomStudent == studentEnrolled.IdHomeroomStudent)
                                .OrderByDescending(e => e.DateIn)
                                .FirstOrDefault();

                            if (attendanceEntry != null)
                                vm.TotalStudent++;
                        }

                        if (vm.TotalStudent > 0)
                            list.Add(vm);
                    }
                }
            }

            var dataPending = list.AsQueryable();

            if (mappingAttendance.AbsentTerms == AbsentTerm.Session)
            {
                if (!string.IsNullOrEmpty(param.Search))
                    dataPending = dataPending
                                    .Where(e => e.ClassID.ToLower().Contains(param.Search.ToLower())
                                    || !string.IsNullOrEmpty(e.Teacher.Id) == false ||
                                    e.Teacher.Id.ToLower().Contains(param.Search.ToLower())
                                    || !string.IsNullOrEmpty(e.Teacher.Description) == false ||
                                    e.Teacher.Description.ToLower().Contains(param.Search.ToLower())
                                    || !string.IsNullOrEmpty(e.Homeroom.Id) == false ||
                                    e.Homeroom.Id.ToLower().Contains(param.Search.ToLower())
                                    || !string.IsNullOrEmpty(e.Homeroom.Description) == false ||
                                    e.Homeroom.Description.ToLower().Contains(param.Search.ToLower())
                        );
            }
            else
            {
                if (!string.IsNullOrEmpty(param.Search))
                    dataPending = dataPending
                                    .Where(e => !string.IsNullOrEmpty(e.Teacher.Description) == false ||
                                    e.Teacher.Description.ToLower().Contains(param.Search.ToLower())
                                    || !string.IsNullOrEmpty(e.Teacher.Id) == false ||
                                    e.Teacher.Id.ToLower().Contains(param.Search.ToLower())
                                    || !string.IsNullOrEmpty(e.Homeroom.Description) == false ||
                                    e.Homeroom.Description.ToLower().Contains(param.Search.ToLower()));
            }
            switch (param.OrderBy)
            {
                case "date":
                    dataPending = param.OrderType == OrderType.Desc
                        ? dataPending.OrderByDescending(x => x.Date)
                        : dataPending.OrderBy(x => x.Date);
                    break;
                case "clasId":
                    dataPending = param.OrderType == OrderType.Desc
                        ? dataPending.OrderByDescending(x => x.ClassID)
                        : dataPending.OrderBy(x => x.ClassID);
                    break;
                case "teacher":
                    dataPending = param.OrderType == OrderType.Desc
                        ? dataPending.OrderByDescending(x => x.Teacher.Description)
                        : dataPending.OrderBy(x => x.Teacher.Description);
                    break;
                case "homeroom":
                    dataPending = param.OrderType == OrderType.Desc
                        ? dataPending.OrderByDescending(x => x.Homeroom.Description)
                        : dataPending.OrderBy(x => x.Homeroom.Description);
                    break;
                case "subjectId":
                    dataPending = param.OrderType == OrderType.Desc
                        ? dataPending.OrderByDescending(x => x.SubjectId)
                        : dataPending.OrderBy(x => x.SubjectId);
                    break;
                case "sessionId":
                    dataPending = param.OrderType == OrderType.Desc
                        ? dataPending.OrderByDescending(x => x.Session.SessionID)
                        : dataPending.OrderBy(x => x.Session.SessionID);
                    break;
                case "totalstudent":
                    dataPending = param.OrderType == OrderType.Desc
                        ? dataPending.OrderByDescending(x => x.TotalStudent)
                        : dataPending.OrderBy(x => x.TotalStudent);
                    break;
            }

            IReadOnlyList<IItemValueVm> items;
            var getAttendanceSummaryPendingResults = dataPending.ToList();

            items = param.Return == CollectionType.Lov? getAttendanceSummaryPendingResults.ToList() :
                getAttendanceSummaryPendingResults
                .SetPagination(param)
                .ToList();

            var count = param.CanCountWithoutFetchDb(items.Count)
            ? items.Count
            : getAttendanceSummaryPendingResults.Select(x => x.Date).Count();

            return Request.CreateApiResult2(items as object,
                param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
