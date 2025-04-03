using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Abstractions;
using BinusSchool.Attendance.FnAttendance.Models;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceRecap;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceRecap
{
    public class GetUnsubmittedAttendaceRecapHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceSummaryService _attendanceSummaryService;
        private readonly IAttendanceRecapService _attendanceRecapService;

        public GetUnsubmittedAttendaceRecapHandler(IAttendanceDbContext dbContext, IAttendanceSummaryService attendanceSummaryService, IAttendanceRecapService attendanceRecapService)
        {
            _dbContext = dbContext;
            _attendanceSummaryService = attendanceSummaryService;
            _attendanceRecapService = attendanceRecapService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailAttendanceRecapRequest>();

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

            var listHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>().Include(x => x.Homeroom)
                .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear && x.IdStudent == param.IdStudent)
                .Select(x => x.Id)
                .ToListAsync(CancellationToken);

            var msHomeroomStudentEnrollment = await _attendanceRecapService.GetHomeroomStudentEnrollmentAsync(param.IdAcademicYear, param.IdLevel, param.IdStudent, CancellationToken);
            var trHomeroomStudentEnrollment = await _attendanceRecapService.GetTrHomeroomStudentEnrollmentAsync(param.IdAcademicYear, param.IdLevel, param.IdStudent, CancellationToken);
            var period = await GetPeriodAsync(param.IdAcademicYear, param.IdLevel, CancellationToken);

            var homeroomStudentEnrollment = msHomeroomStudentEnrollment.Union(trHomeroomStudentEnrollment);

            if (period == null)
            {
                throw new BadRequestException("Period not found !");
            }

            var startDate = period.OrderBy(x => x.StartDate).FirstOrDefault().StartDate;
            var endDate = period.OrderByDescending(x => x.EndDate).FirstOrDefault().EndDate;

            var lessons = homeroomStudentEnrollment.Select(x => x.IdLesson).Distinct().ToList();

            var scheduleLessons = await _attendanceRecapService.GetScheduleLessonAsync(param.IdAcademicYear, param.IdLevel, lessons, startDate, endDate, CancellationToken);

            if (!scheduleLessons.Any())
            {
                return Request.CreateApiResult2();
            }

            var listMappingSemesterGradeIdClassroomAndIdHomeroom =
                new List<(
                    int Semester,
                    string IdGrade,
                    string IdClassroom,
                    string ClassroomCode,
                    string IdHomeroom,
                    string GradeCode)>();

            var homeroomQueryable = _dbContext.Entity<MsHomeroom>()
                .AsNoTracking()
                .Where(e => e.IdGrade == param.IdGrade && e.Grade.Level.Id == param.IdLevel)
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
            {
                listMappingSemesterGradeIdClassroomAndIdHomeroom.Add((
                    item.Semester,
                    item.IdGrade,
                    item.IdClassroom,
                    item.ClassroomCode,
                    item.IdHomeroom,
                    item.GradeCode));
            }

            var dict = new Dictionary<(int Semester, string IdGrade, string IdHomeroom), List<StudentEnrollmentDto2>>();
            foreach (var item in listMappingSemesterGradeIdClassroomAndIdHomeroom)
                dict.Add((item.Semester, item.IdGrade, item.IdHomeroom),
                    await _attendanceSummaryService.GetStudentEnrolledAsync(
                        item.IdHomeroom,
                        DateTime.MinValue, CancellationToken));
            var listIdStudent = dict.SelectMany(e => e.Value).Where(x => x.IdStudent == param.IdStudent).Select(e => e.IdStudent).Distinct().ToArray();
            var studentStatuses =
                await _attendanceSummaryService.GetStudentStatusesAsync(listIdStudent, param.IdAcademicYear,
                    CancellationToken);

            var chunkScheduleLessons = scheduleLessons.ChunkBy(5000);
            var groupedAttendanceEntries = new Dictionary<string, List<Models.AttendanceEntryResult>>();
            foreach (var (item, i) in chunkScheduleLessons.Select((value, i) => (value, i)))
            {
                var groupedAttendanceEntriesChunk = await _attendanceSummaryService.GetAttendanceEntriesGroupedAsync(
                    item
                        .Select(e => e.Id)
                        .ToArray(), CancellationToken);

                groupedAttendanceEntries.AddRange(groupedAttendanceEntriesChunk);

            }

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

            var lists = new List<GetAttendanceSummaryUnsubmitedResult>();

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

                        foreach (var homeroom in listMappingSemesterGradeIdClassroomAndIdHomeroom)
                        {
                            if (!(homeroom.Semester == item2.Semester &&
                                  item2.IdGrade == homeroom.IdGrade &&
                                  item2.LessonPathwayResults.Any(e => e.IdHomeroom == homeroom.IdHomeroom)))
                                continue;

                            if (!string.IsNullOrWhiteSpace(param.IdHomeroom) && param.IdHomeroom != homeroom.IdClassroom)
                                continue;

                            var vm = new GetAttendanceSummaryUnsubmitedResult
                            {
                                Date = item2.ScheduleDate,
                                ClassID = item2.ClassID,
                                Teacher = new ItemValueVm
                                {
                                    Id = teacher?.Teacher.IdUser,
                                    Description = teacher is null
                                        ? string.Empty
                                        : NameUtil.GenerateFullName(teacher.Teacher.FirstName,
                                            teacher.Teacher.LastName) ?? string.Empty
                                },
                                Homeroom = new ItemValueVm
                                {
                                    Id = homeroom.IdHomeroom ?? string.Empty,
                                    Description = homeroom.GradeCode + homeroom.ClassroomCode ?? string.Empty
                                },
                                SubjectId = item2.Subject.SubjectID,
                                Session = new RedisAttendanceSummarySession
                                {
                                    Id = item2.Session.Id,
                                    Name = item2.Session.Name,
                                    SessionID = item2.Session.SessionID
                                }
                            };
                            vm.ListStudent = new List<string>();

                            if (!dict.ContainsKey((homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)))
                                continue;

                            var listStudentEnrolled = dict[(homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)];

                            // get all attendance entry by id schedule lesson
                            if (!groupedAttendanceEntries.ContainsKey(item2.Id))
                            {
                                //loop every student enrolled
                                foreach (var studentEnrolled in listStudentEnrolled)
                                {
                                    //logic student status
                                    var studentStatus = studentStatuses
                                        .FirstOrDefault(e => e.IdStudent == studentEnrolled.IdStudent && e.StartDt.Date <= item2.ScheduleDate.Date && e.EndDt >= item2.ScheduleDate.Date);
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

                                        if (current.Ignored || current.IdLesson != item2.IdLesson)
                                            continue;

                                        //when current id lesson is same as above and the date of the current moving still satisfied
                                        //then set to passed, other than that will be excluded
                                        if (current.StartDt.Date <= item2.ScheduleDate.Date &&
                                            item2.ScheduleDate.Date < current.EndDt.Date)
                                            passed = true;
                                    }

                                    if (!passed)
                                        continue;

                                    vm.TotalStudent++;
                                    vm.ListStudent.Add(studentEnrolled.IdStudent);
                                }
                            }
                            else
                            {
                                var attendanceEntries = groupedAttendanceEntries[item2.Id];

                                //loop every student enrolled
                                foreach (var studentEnrolled in listStudentEnrolled)
                                {
                                    //logic student status
                                    var studentStatus = studentStatuses
                                        .FirstOrDefault(e => e.IdStudent == studentEnrolled.IdStudent && e.StartDt.Date <= item2.ScheduleDate.Date && e.EndDt >= item2.ScheduleDate.Date);
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

                                        if (current.Ignored || current.IdLesson != item2.IdLesson)
                                            continue;

                                        //when current id lesson is same as above and the date of the current moving still satisfied
                                        //then set to passed, other than that will be excluded
                                        if (current.StartDt.Date <= item2.ScheduleDate.Date &&
                                            item2.ScheduleDate.Date < current.EndDt.Date)
                                            passed = true;
                                    }

                                    if (!passed)
                                        continue;
                                    var attendanceEntry = attendanceEntries.Where(e =>
                                            e.IdHomeroomStudent == studentEnrolled.IdHomeroomStudent)
                                        .OrderByDescending(e => e.DateIn)
                                        .FirstOrDefault();

                                    if (attendanceEntry is null)
                                    {
                                        vm.TotalStudent++;
                                        vm.ListStudent.Add(studentEnrolled.IdStudent);
                                    }
                                }
                            }

                            if (vm.TotalStudent > 0)
                                lists.Add(vm);
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

                    foreach (var homeroom in listMappingSemesterGradeIdClassroomAndIdHomeroom)
                    {
                        if (!(homeroom.Semester == item2.Semester &&
                              item2.IdGrade == homeroom.IdGrade &&
                              item2.LessonPathwayResults.Any(e => e.IdHomeroom == homeroom.IdHomeroom)))
                            continue;

                        var vm = new GetAttendanceSummaryUnsubmitedResult
                        {
                            Date = item2.ScheduleDate,
                            ClassID = item2.ClassID,
                            Teacher = new ItemValueVm
                            {
                                Id = teacher?.Teacher.IdUser,
                                Description = teacher is null
                                    ? string.Empty
                                    : NameUtil.GenerateFullName(teacher.Teacher.FirstName, teacher.Teacher.LastName) ??
                                      string.Empty
                            },
                            Homeroom = new ItemValueVm
                            {
                                Id = homeroom.IdHomeroom ?? string.Empty,
                                Description = homeroom.GradeCode + homeroom.ClassroomCode ?? string.Empty
                            },
                            SubjectId = item2.Subject.SubjectID,
                            Session = new RedisAttendanceSummarySession
                            {
                                Id = item2.Session.Id,
                                Name = item2.Session.Name,
                                SessionID = item2.Session.SessionID
                            }
                        };
                        vm.ListStudent = new List<string>();

                        if (!dict.ContainsKey((homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)))
                            continue;

                        var listStudentEnrolled = dict[(homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)];

                        // get all attendance entry by id schedule lesson
                        if (!groupedAttendanceEntries.ContainsKey(item2.Id))
                        {
                            //loop every student enrolled
                            foreach (var studentEnrolled in listStudentEnrolled)
                            {
                                //logic student status
                                var studentStatus = studentStatuses
                                   .FirstOrDefault(e => e.IdStudent == studentEnrolled.IdStudent && e.StartDt.Date <= item2.ScheduleDate.Date && e.EndDt >= item2.ScheduleDate.Date);
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

                                    if (current.Ignored || current.IdLesson != item2.IdLesson)
                                        continue;

                                    //when current id lesson is same as above and the date of the current moving still satisfied
                                    //then set to passed, other than that will be excluded
                                    if (current.StartDt.Date <= item2.ScheduleDate.Date &&
                                        item2.ScheduleDate.Date < current.EndDt.Date)
                                        passed = true;
                                }

                                if (!passed)
                                    continue;

                                vm.TotalStudent++;
                                vm.ListStudent.Add(studentEnrolled.IdStudent);
                            }
                        }
                        else
                        {
                            var attendanceEntries = groupedAttendanceEntries[item2.Id];

                            //loop every student enrolled
                            foreach (var studentEnrolled in listStudentEnrolled)
                            {
                                //logic student status
                                var studentStatus = studentStatuses
                                    .FirstOrDefault(e => e.IdStudent == studentEnrolled.IdStudent && e.StartDt.Date <= item2.ScheduleDate.Date && e.EndDt >= item2.ScheduleDate.Date);
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

                                    if (current.Ignored || current.IdLesson != item2.IdLesson)
                                        continue;

                                    //when current id lesson is same as above and the date of the current moving still satisfied
                                    //then set to passed, other than that will be excluded
                                    if (current.StartDt.Date <= item2.ScheduleDate.Date &&
                                        item2.ScheduleDate.Date < current.EndDt.Date)
                                        passed = true;
                                }

                                if (!passed)
                                    continue;
                                var attendanceEntry = attendanceEntries.Where(e =>
                                        e.IdHomeroomStudent == studentEnrolled.IdHomeroomStudent)
                                    .OrderByDescending(e => e.DateIn)
                                    .FirstOrDefault();

                                if (attendanceEntry is null)
                                {
                                    vm.TotalStudent++;
                                    vm.ListStudent.Add(studentEnrolled.IdStudent);
                                }
                            }
                        }

                        if (vm.TotalStudent > 0)
                            lists.Add(vm);
                    }
                }
            }

            var dataUnsubmited = lists.AsQueryable();

            if (!string.IsNullOrEmpty(param.Search))
            {
                dataUnsubmited = dataUnsubmited.Where(x => x.Date.ToString().Contains(param.Search) || x.ClassID.ToLower().Contains(param.Search.ToLower()) || x.Session.Name.ToLower().Contains(param.Search.ToLower()));
            }

            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                switch (param.OrderBy.ToLower())
                {
                    case "date":
                        dataUnsubmited = dataUnsubmited = param.OrderType == OrderType.Desc
                            ? dataUnsubmited.OrderByDescending(x => x.Date)
                            : dataUnsubmited.OrderBy(x => x.Date);
                        break;
                    case "classid":
                        dataUnsubmited = param.OrderType == OrderType.Desc
                            ? dataUnsubmited.OrderByDescending(x => x.ClassID)
                            : dataUnsubmited.OrderBy(x => x.ClassID);
                        break;
                    case "session":
                        dataUnsubmited = dataUnsubmited = param.OrderType == OrderType.Desc
                           ? dataUnsubmited.OrderByDescending(x => x.Session.Name)
                           : dataUnsubmited.OrderBy(x => x.Session.Name);
                        break;
                    default:
                        dataUnsubmited = dataUnsubmited.OrderByDescending(x => x.Date);
                        break;
                }
            }

            IReadOnlyList<IItemValueVm> items;
            var getAttendanceSummaryUnsubmitedResults = dataUnsubmited.ToList();

            if (param.Return == CollectionType.Lov)
            {
                items = getAttendanceSummaryUnsubmitedResults
                       .ToList();
            }
            else
            {
                items = getAttendanceSummaryUnsubmitedResults
                       .SetPagination(param)
                       .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : getAttendanceSummaryUnsubmitedResults.Select(x => x.Date).Count();

            return Request.CreateApiResult2(items as object,
                param.CreatePaginationProperty(count).AddColumnProperty());
        }

        public async Task<Dictionary<string, List<AttendanceEntryResult>>> GetAttendanceEntriesGroupedAsync(
            List<string> idSchedules, string idStudent, CancellationToken cancellationToken)
        {
            var results = await _dbContext.Entity<TrAttendanceEntryV2>()
                .Include(x => x.HomeroomStudent)
                .Where(e => idSchedules.Contains(e.IdScheduleLesson) && e.HomeroomStudent.IdStudent == idStudent)
                .Select(e => new AttendanceEntryResult
                {
                    IdScheduleLesson = e.IdScheduleLesson,
                    IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
                    Status = e.Status,
                    IsFromAttendanceAdministration = e.IsFromAttendanceAdministration,
                    //PositionIn = e.PositionIn,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    DateIn = e.DateIn.Value,
                }).ToListAsync(cancellationToken);

            return results.GroupBy(e => e.IdScheduleLesson).ToDictionary(g => g.Key, g => g.ToList());
        }
        public async Task<List<PeriodResult>> GetPeriodAsync(string idAcademicYear, string idLevel, CancellationToken cancellationToken)
        {
            var queryable = _dbContext.Entity<MsPeriod>()
                .AsNoTracking()
                .AsQueryable();

            if (string.IsNullOrWhiteSpace(idAcademicYear))
                throw new InvalidOperationException();

            if (!string.IsNullOrWhiteSpace(idLevel))
                queryable = queryable.Where(e => e.Grade.IdLevel == idLevel);

            queryable = queryable.Where(e => e.Grade.Level.IdAcademicYear == idAcademicYear);

            var listPeriod = await queryable
                .GroupBy(e => new PeriodResult
                {
                    IdPeriod = e.Id,
                    IdGrade = e.IdGrade,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Semester = e.Semester,
                    IdLevel = e.Grade.IdLevel,
                    AttendanceStartDate = e.AttendanceStartDate,
                    AttendanceEndDate = e.AttendanceEndDate
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            return listPeriod;
        }
    }
}
