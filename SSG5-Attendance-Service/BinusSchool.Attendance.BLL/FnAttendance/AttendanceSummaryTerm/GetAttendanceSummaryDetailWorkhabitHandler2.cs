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
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Utils;
using BinusSchool.Attendance.FnAttendance.Models;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailWorkhabitHandler2 : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceSummaryRedisService _attendanceSummaryRedisService;
        private readonly IAttendanceSummaryService _attendanceSummaryService;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceSummary _apiAttendanceSummary;
        private readonly ILogger<GetAttendanceSummaryDetailSchoolDayHandler2> _logger;
        private readonly IMachineDateTime _datetime;

        public GetAttendanceSummaryDetailWorkhabitHandler2(
            IAttendanceSummaryRedisService attendanceSummaryRedisService,
            IAttendanceSummaryService attendanceSummaryService,
            IAttendanceDbContext dbContext,
            IAttendanceSummary apiAttendanceSummary,
            ILogger<GetAttendanceSummaryDetailSchoolDayHandler2> logger,
            IMachineDateTime dateTime)
        {
            _attendanceSummaryRedisService = attendanceSummaryRedisService;
            _attendanceSummaryService = attendanceSummaryService;
            _dbContext = dbContext;
            _apiAttendanceSummary = apiAttendanceSummary;
            _logger = logger;
            _datetime = dateTime;
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
            var param = Request.ValidateParams<GetAttendanceSummaryDetailWorkhabitRequest>();
            var _columns = new[] { "date", "session", "subject", "teacherName", "comment" };

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

            var GetHomeroomTeacher = new List<HomeroomTeacherResult>();
            if (mappingAttendance.AbsentTerms == AbsentTerm.Day)
                GetHomeroomTeacher = await _attendanceSummaryService.GetHomeroomTeacherAsync(param.IdAcademicYear, param.IdLevel, CancellationToken);

            //Get Student Status
            var listIdStudent = new List<string> { param.IdStudent }.ToArray();
            var studentStatuses = await _attendanceSummaryService.GetStudentStatusesAsync(listIdStudent, param.IdAcademicYear, CancellationToken);

            // Get Student Enrollment
            var getStudentEnrollment = await _attendanceSummaryService.GetStudentEnrolledByStudentAsync(param.IdAcademicYear,
                        param.IdStudent,
                        DateTime.MinValue, CancellationToken);

            // Get Entry Student
            var getAttendanceEntries = await _attendanceSummaryService.GetAttendanceEntryByStudentAsync(param.IdAcademicYear, param.IdStudent, param.StartDate, param.EndDate, CancellationToken);

            //get lesson teacher from lesson attendance entry
            var listLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                     .Include(e => e.Lesson)
                                     .Where(e => e.Lesson.IdAcademicYear == param.IdAcademicYear
                                             && e.IsAttendance
                                             && getAttendanceEntries.Select(x => x.IdLesson).Distinct().ToList().Contains(e.IdLesson)
                                             && e.IsPrimary)
                                     .ToListAsync(CancellationToken);

            var listAttendanceEntry = getAttendanceEntries
                                       .Where(e => e.AttendanceEntryWorkhabit.Any(f => f.IdMappingAttendanceWorkhabit == param.IdMappingAttendanceWorkhabit)
                                                && e.Status == AttendanceEntryStatus.Submitted
                                            )
                                        .ToList();

            //set user entry from attendance administration from lesson teacher
            foreach (var data in listAttendanceEntry.Where(x => x.IsFromAttendanceAdministration).ToList())
            {
                data.IdUserAttendanceEntry = listLessonTeacher.Where(x => x.IdLesson == data.IdLesson).Select(x => x.IdUser).FirstOrDefault();
            }

            var listIdUserTeacher = listAttendanceEntry.Select(e => e.IdUserAttendanceEntry).Distinct().ToList();

            var listUser = await _dbContext.Entity<MsUser>()
                        .Where(e => listIdUserTeacher.Contains(e.Id))
                        .Select(e => new
                        {
                            Id = e.Id,
                            DisplayName = e.DisplayName
                        })
                        .ToListAsync(CancellationToken);

            //start logic
            var listWorkhabit = new List<GetAttendanceSummaryDetailWorkhabitResult>();
            foreach (var itemAttendanceEntry in listAttendanceEntry)
            {
                //filter student status
                if (!studentStatuses.Any(e => e.StartDt.Date <= itemAttendanceEntry.ScheduleDate.Date && e.EndDt >= itemAttendanceEntry.ScheduleDate.Date && e.IdStudent == param.IdStudent))
                    continue;

                var teacherName = listUser.Where(e => e.Id == itemAttendanceEntry.IdUserAttendanceEntry).Select(e => e.DisplayName).FirstOrDefault();

                var studentEnrolled = getStudentEnrollment.Where(e => e.IdHomeroomStudent == itemAttendanceEntry.IdHomeroomStudent).FirstOrDefault();

                var passed = false;
                //logic moving
                foreach (var current in studentEnrolled.Items)
                {
                    if (passed)
                        break;

                    if (string.IsNullOrWhiteSpace(current.IdLesson))
                        continue;

                    if (current.Ignored || current.IdLesson != itemAttendanceEntry.IdLesson)
                        continue;

                    //when current id lesson is same as above and the date of the current moving still satisfied
                    //then set to passed, other than that will be excluded
                    if (current.StartDt.Date <= itemAttendanceEntry.ScheduleDate.Date &&
                        itemAttendanceEntry.ScheduleDate.Date < current.EndDt.Date)
                        passed = true;
                }

                if (!passed)
                    continue;

                var listHomeroomTeacher = GetHomeroomTeacher.Where(e => e.IdHomeroom == studentEnrolled.IdHomeroom)
                                            .Where(e => e.Position.Code == PositionConstant.ClassAdvisor)
                                            .Select(e => new
                                            {
                                                name = NameUtil.GenerateFullName(e.Teacher.FirstName, e.Teacher.LastName)
                                            })
                                            .ToList();

                var newWorkhabit = new GetAttendanceSummaryDetailWorkhabitResult
                {
                    Date = itemAttendanceEntry.ScheduleDate,
                    Session = itemAttendanceEntry.Session.Name,
                    Subject = itemAttendanceEntry.Subject.Description,
                    TeacherName = teacherName,
                    HomeroomTeacherName = String.Join(", ", listHomeroomTeacher.Select(e => e.name).ToList()),
                    Comment = itemAttendanceEntry.Notes,
                };

                listWorkhabit.Add(newWorkhabit);
            }
            var queryWorkhabit = listWorkhabit.Distinct();


            queryWorkhabit = mappingAttendance.AbsentTerms == AbsentTerm.Session
                ? queryWorkhabit = queryWorkhabit
                            .GroupBy(e => new
                            {
                                Date = e.Date,
                                Session = e.Session,
                                Subject = e.Subject,
                                TeacherName = e.TeacherName,
                                Comment = e.Comment,
                            })
                            .Select(e => new GetAttendanceSummaryDetailWorkhabitResult
                            {
                                Date = e.Key.Date,
                                Session = e.Key.Session,
                                Subject = e.Key.Subject,
                                TeacherName = e.Key.TeacherName,
                                Comment = e.Key.Comment,
                            }).OrderBy(e => e.Date).Distinct()
                :queryWorkhabit = queryWorkhabit
                            .GroupBy(e => new
                            {
                                Date = e.Date,
                                TeacherName = e.HomeroomTeacherName,
                                Comment = e.Comment,
                            })
                            .Select(e => new GetAttendanceSummaryDetailWorkhabitResult
                            {
                                Date = e.Key.Date,
                                TeacherName = e.Key.TeacherName,
                                Comment = e.Key.Comment,
                            }).OrderBy(e => e.Date).Distinct();


            switch (param.OrderBy)
            {
                case "date":
                    queryWorkhabit = param.OrderType == OrderType.Desc
                        ? queryWorkhabit.OrderByDescending(x => x.Date)
                        : queryWorkhabit.OrderBy(x => x.Date);
                    break;
                case "session":
                    queryWorkhabit = param.OrderType == OrderType.Desc
                        ? queryWorkhabit.OrderByDescending(x => x.Session)
                        : queryWorkhabit.OrderBy(x => x.Session);
                    break;
                case "subject":
                    queryWorkhabit = param.OrderType == OrderType.Desc
                        ? queryWorkhabit.OrderByDescending(x => x.Subject)
                        : queryWorkhabit.OrderBy(x => x.Subject);
                    break;
                case "teacherName":
                    queryWorkhabit = param.OrderType == OrderType.Desc
                        ? queryWorkhabit.OrderByDescending(x => x.TeacherName)
                        : queryWorkhabit.OrderBy(x => x.TeacherName);
                    break;
                case "commentt":
                    queryWorkhabit = param.OrderType == OrderType.Desc
                        ? queryWorkhabit.OrderByDescending(x => x.Comment)
                        : queryWorkhabit.OrderBy(x => x.Comment);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = queryWorkhabit
                    .ToList();
            }
            else
            {
                items = queryWorkhabit
                    .SetPagination(param)
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : queryWorkhabit.Select(x => x.Date).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
