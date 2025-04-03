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
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
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
    public class GradeVm
    {
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdClassroom { get; set; }
    }

    public class GetAttendanceSummaryDetailSchoolDayHandler2 : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceSummaryRedisService _attendanceSummaryRedisService;
        private readonly IAttendanceSummaryService _attendanceSummaryService;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceSummary _apiAttendanceSummary;
        private readonly ILogger<GetAttendanceSummaryDetailSchoolDayHandler2> _logger;

        public GetAttendanceSummaryDetailSchoolDayHandler2(
            IAttendanceSummaryRedisService attendanceSummaryRedisService,
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
            var param = Request.ValidateParams<GetAttendanceSummaryDetailSchoolDayRequest>();
            var columns = new[] { "Subject", "session", "classId", "homeroom" };

            var filterIdHomerooms = new GetHomeroomByIdUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                SelectedPosition = param.SelectedPosition,
                IdUser = param.IdUser,
                IdLevel = param.IdLevel,
                IdGrade = param.IdGrade,
                IdClassroom = param.IdClassroom,
                ClassId = param.ClassId
            };

            var listIdLesson =
                await _attendanceSummaryRedisService.GetLessonByUser(filterIdHomerooms, CancellationToken);

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

            var attendanceAndWorkhabitByLevel = await _apiAttendanceSummary.GetAttendanceAndWorkhabitByLevel(
                new GetAttendanceAndWorkhabitByLevelRequest
                {
                    IdLevel = param.IdLevel
                });

            var attendanceAndWorkhabitByLevelPayload = attendanceAndWorkhabitByLevel.Payload;

            var queryAttendanceMappingAttendance = _dbContext.Entity<MsAttendanceMappingAttendance>()
                .Include(x => x.MappingAttendance)
                .Include(x => x.Attendance)
                .Where(x => x.Attendance.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.IdLevel))
                queryAttendanceMappingAttendance =
                    queryAttendanceMappingAttendance.Where(e => e.MappingAttendance.IdLevel == param.IdLevel);

            var getAttendanceMappingAttendance = await queryAttendanceMappingAttendance
                .Select(e => new RedisAttendanceSummaryAttendanceMappingAttendanceResult
                {
                    Id = e.Id,
                    AbsenceCategory = e.Attendance.AbsenceCategory,
                })
                .ToListAsync(CancellationToken);

            var listAttendanceAndWorkhabit = attendanceAndWorkhabitByLevelPayload.Attendances.Select(e =>
                new GetAttendanceAndWorkhabitResult
                {
                    Id = e.Id,
                    Code = e.Code,
                    Description = e.Description,
                    Type = "Attendance",
                    AbsenceCategory = getAttendanceMappingAttendance.Where(f => f.Id == e.Id)
                        .Select(y => y.AbsenceCategory).FirstOrDefault()
                }).ToList();

            listAttendanceAndWorkhabit.AddRange(attendanceAndWorkhabitByLevelPayload.Workhabits.Select(e =>
                new GetAttendanceAndWorkhabitResult
                {
                    Id = e.Id,
                    Code = e.Code,
                    Description = e.Description,
                    Type = "Workhabits",
                }).ToList());

            listAttendanceAndWorkhabit.Add(new GetAttendanceAndWorkhabitResult
            {
                Id = "",
                Code = "Unsubmited",
                Description = "Unsubmited",
                Total = 0,
                Type = "Attendance",
                Students = new List<GetStudentAttendance>()
            });

            listAttendanceAndWorkhabit.Add(new GetAttendanceAndWorkhabitResult
            {
                Id = "",
                Code = "Pending",
                Description = "Pending",
                Total = 0,
                Type = "Attendance",
                Students = new List<GetStudentAttendance>()
            });

            //get short listed data
            var scheduleLessonAsQueryable = (await _attendanceSummaryRedisService.GetScheduleLessonAsync(
                    param.IdAcademicYear,
                    param.IdLevel, CancellationToken))
                .Where(e => e.ScheduleDate.Date >= param.StartDate.Date && e.ScheduleDate.Date <= param.EndDate.Date)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(param.IdSession))
                scheduleLessonAsQueryable = scheduleLessonAsQueryable.Where(e => e.Session.Id == param.IdSession);

            if (!string.IsNullOrEmpty(param.IdSession))
                scheduleLessonAsQueryable = scheduleLessonAsQueryable.Where(e => e.Session.Id == param.IdSession);

            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                scheduleLessonAsQueryable = scheduleLessonAsQueryable.Where(e => e.Semester == param.Semester);

            if (!string.IsNullOrEmpty(param.ClassId))
                scheduleLessonAsQueryable = scheduleLessonAsQueryable.Where(e => e.ClassID == param.ClassId);

            if (!string.IsNullOrEmpty(param.IdGrade))
                scheduleLessonAsQueryable = scheduleLessonAsQueryable.Where(e => e.IdGrade == param.IdGrade);

            if (listIdLesson.Any())
                scheduleLessonAsQueryable = scheduleLessonAsQueryable.Where(e => listIdLesson.Contains(e.IdLesson));

            var scheduleLessons = scheduleLessonAsQueryable
                .GroupBy(e => new
                {
                    e.IdLesson
                })
                .ToDictionary(e => e.Key, y => y.ToList());

            if (!scheduleLessons.Any())
                return Request.CreateApiResult2(new List<GetAttendanceSummaryDetailSchoolDayResult>() as object,
                    param.CreatePaginationProperty(0).AddColumnProperty(columns));

            var listMappingSemesterGradeIdClassroomAndIdHomeroom =
                new List<(int Semester, string IdGrade, string IdClassroom, string ClassroomCode, string IdHomeroom,
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

            if (!string.IsNullOrEmpty(param.IdClassroom))
                homeroomQueryable =
                    homeroomQueryable.Where(e => e.GradePathwayClassroom.Classroom.Id == param.IdClassroom);

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

            var results = new List<GetAttendanceSummaryDetailSchoolDayResult>();

            foreach (var item in scheduleLessons)
            {
                if (!item.Value.Any())
                    continue;

                // list student enrolled
                foreach (var item2 in item.Value)
                {
                    foreach (var homeroom in listMappingSemesterGradeIdClassroomAndIdHomeroom)
                    {
                        if (!(homeroom.Semester == item2.Semester &&
                              item2.IdGrade == homeroom.IdGrade &&
                              item2.LessonPathwayResults.Any(e => e.IdHomeroom == homeroom.IdHomeroom)))
                            continue;

                        var vm = new GetAttendanceSummaryDetailSchoolDayResult
                        {
                            Date = item2.ScheduleDate,
                            Session = new ItemValueVm
                            {
                                Id = item2.Session.Id,
                                Description = item2.Session.SessionID,
                            },
                            ClassId = item2.ClassID,
                            Homeroom = homeroom.GradeCode + homeroom.ClassroomCode
                        };

                        var listStudentEnrolled = dict[(homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)];

                        foreach (var e in listAttendanceAndWorkhabit)
                            vm.DataAttendanceAndWorkhabit.Add(new GetAttendanceAndWorkhabitResult
                            {
                                Id = e.Id,
                                Code = e.Code,
                                Description = e.Description,
                                Type = e.Type,
                                AbsenceCategory = e.AbsenceCategory,
                                Total = e.Total,
                            });

                        if (mappingAttendance.AbsentTerms == AbsentTerm.Day)
                            if (results.Any(e => e.Date.Date == item2.ScheduleDate.Date && e.Homeroom == vm.Homeroom))
                                continue;

                        if (!dict.ContainsKey((homeroom.Semester, homeroom.IdGrade, homeroom.IdHomeroom)))
                            continue;

                        // get all attendance entry by id schedule lesson
                        if (!groupedAttendanceEntries.ContainsKey(item2.Id))
                        {
                            // no attendance entries, then set all student enrolled to unsubmitted by default
                            foreach (var studentEnrolled in listStudentEnrolled)
                            {
                                var habit = vm.DataAttendanceAndWorkhabit.First(e => e.Code == "Unsubmited");
                                habit.Total++;
                                habit.Students.Add(new GetStudentAttendance
                                {
                                    Student = new NameValueVm
                                    {
                                        Id = studentEnrolled.IdStudent,
                                        Name = studentEnrolled.FullName
                                    },
                                    Status = "Unsubmited"
                                });
                            }

                            results.Add(vm);
                            continue;
                        }

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

                                if (current.Ignored || current.IdLesson != item.Key.IdLesson)
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
                                var habit = vm.DataAttendanceAndWorkhabit.First(e => e.Code == "Unsubmited");
                                habit.Total++;
                                habit.Students.Add(new GetStudentAttendance
                                {
                                    Student = new NameValueVm
                                    {
                                        Id = studentEnrolled.IdStudent,
                                        Name = studentEnrolled.FullName
                                    },
                                    Status = "Unsubmited"
                                });
                            }
                            else
                            {
                                if (attendanceEntry.Status == AttendanceEntryStatus.Pending)
                                {
                                    var habit = vm.DataAttendanceAndWorkhabit.First(e => e.Code == "Pending");
                                    habit.Total++;
                                    habit.Students.Add(new GetStudentAttendance
                                    {
                                        Student = new NameValueVm
                                        {
                                            Id = studentEnrolled.IdStudent,
                                            Name = studentEnrolled.FullName
                                        },
                                        Status = "Pending"
                                    });
                                }

                                foreach (var itemAttendanceAndWorkhabit in vm.DataAttendanceAndWorkhabit)
                                {
                                    if (itemAttendanceAndWorkhabit.Type == "Attendance")
                                    {
                                        if (attendanceEntry.IdAttendanceMappingAttendance ==
                                            itemAttendanceAndWorkhabit.Id)
                                        {
                                            itemAttendanceAndWorkhabit.Total++;
                                            itemAttendanceAndWorkhabit.Students.Add(new GetStudentAttendance
                                            {
                                                Student = new NameValueVm
                                                {
                                                    Id = studentEnrolled.IdStudent,
                                                    Name = studentEnrolled.FullName
                                                },
                                                Status = itemAttendanceAndWorkhabit.Description
                                            });
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (attendanceEntry.AttendanceEntryWorkhabit.Any(e =>
                                                e.IdMappingAttendanceWorkhabit == itemAttendanceAndWorkhabit.Id))
                                        {
                                            itemAttendanceAndWorkhabit.Total++;
                                            itemAttendanceAndWorkhabit.Students.Add(new GetStudentAttendance
                                            {
                                                Student = new NameValueVm
                                                {
                                                    Id = studentEnrolled.IdStudent,
                                                    Name = studentEnrolled.FullName
                                                },
                                                Status = itemAttendanceAndWorkhabit.Description
                                            });
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        results.Add(vm);
                    }
                }
            }

            var querySchoolDay = results.Distinct();

            switch (param.OrderBy)
            {
                case "date":
                    querySchoolDay = param.OrderType == OrderType.Desc
                        ? querySchoolDay.OrderByDescending(x => x.Date)
                        : querySchoolDay.OrderBy(x => x.Date);
                    break;
                case "session":
                    querySchoolDay = param.OrderType == OrderType.Desc
                        ? querySchoolDay.OrderByDescending(x => x.Session.Description)
                        : querySchoolDay.OrderBy(x => x.Session.Description);
                    break;
                case "classId":
                    querySchoolDay = param.OrderType == OrderType.Desc
                        ? querySchoolDay.OrderByDescending(x => x.ClassId)
                        : querySchoolDay.OrderBy(x => x.ClassId);
                    break;
                case "homeroom":
                    querySchoolDay = param.OrderType == OrderType.Desc
                        ? querySchoolDay.OrderByDescending(x => x.Homeroom)
                        : querySchoolDay.OrderBy(x => x.Homeroom);
                    break;
            }

            IReadOnlyList<IItemValueVm> items;
            var getAttendanceSummaryDetailSchoolDayResults = querySchoolDay.ToList();
            if (param.Return == CollectionType.Lov)
            {
                items = getAttendanceSummaryDetailSchoolDayResults.ToList();
            }
            else
            {
                items = getAttendanceSummaryDetailSchoolDayResults
                    .SetPagination(param)
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : getAttendanceSummaryDetailSchoolDayResults.Select(x => x.DataAttendanceAndWorkhabit).Count();

            return Request.CreateApiResult2(items as object,
                param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
