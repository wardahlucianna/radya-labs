using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Abstractions;
using BinusSchool.Attendance.FnAttendance.Models;
using BinusSchool.Attendance.FnAttendance.Services;
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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceRecap
{
    public class GetPendingAttendanceRecapHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAttendanceRecapService _attendanceRecapService;
        private readonly IAttendanceSummaryService _attendanceSummaryService;
        public GetPendingAttendanceRecapHandler(IAttendanceDbContext dbContext, IAttendanceRecapService attendanceRecapService, IAttendanceSummaryService attendanceSummaryService)
        {
            _dbContext = dbContext;
            _attendanceRecapService = attendanceRecapService;
            _attendanceSummaryService = attendanceSummaryService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailAttendanceRecapRequest>();

            //ms mapping attendance
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

            //Get Student Status
            var listIdStudent = new List<string> { param.IdStudent }.ToArray();
            var studentStatuses = await _attendanceSummaryService.GetStudentStatusesAsync(listIdStudent, param.IdAcademicYear, CancellationToken);

            // Get Student Enrollment
            var getStudentEnrollment = await _attendanceSummaryService.GetStudentEnrolledByStudentAsync(param.IdAcademicYear,
                        param.IdStudent,
                        DateTime.MinValue, CancellationToken);

            var period = await GetPeriodAsync(param.IdAcademicYear, param.IdLevel, CancellationToken);

            if (period == null)
            {
                throw new BadRequestException("Period not found !");
            }

            var startDate = period.OrderBy(x => x.StartDate).FirstOrDefault().StartDate;
            var endDate = period.OrderByDescending(x => x.EndDate).FirstOrDefault().EndDate;

            // Get Entry Student
            var getAttendanceEntries = await GetAttendanceEntryByStudentAsync(param.IdAcademicYear, param.IdStudent, startDate, endDate, CancellationToken);
            if (!getAttendanceEntries.Any())
                return Request.CreateApiResult2();

            var listAttendanceEntry = getAttendanceEntries.Where(e => e.Status == AttendanceEntryStatus.Submitted).ToList();

            //start logic
            var listUaEa = new List<GetDataDetailAttendanceRecapResult>();
            foreach (var itemAttendanceEntry in listAttendanceEntry)
            {
                //filter student status
                if (!studentStatuses.Any(e => e.StartDt.Date <= itemAttendanceEntry.ScheduleDate.Date && e.EndDt >= itemAttendanceEntry.ScheduleDate.Date && e.IdStudent == param.IdStudent))
                    continue;

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

                var newUaEa = new GetDataDetailAttendanceRecapResult
                {
                    Date = itemAttendanceEntry.ScheduleDate,
                    Session = itemAttendanceEntry.Session.SessionID,
                    ClassId = itemAttendanceEntry.ClassID,
                    Homeroom = itemAttendanceEntry.Homeroom
                };

                listUaEa.Add(newUaEa);
            }

            var queryUaEa = listUaEa.Distinct();

            queryUaEa = mappingAttendance.AbsentTerms == AbsentTerm.Session
                ? queryUaEa
                            .GroupBy(e => new
                            {
                                Date = e.Date,
                                Session = e.Session,
                                ClassId = e.ClassId,
                                Homeroom = e.Homeroom
                            })
                            .Select(e => new GetDataDetailAttendanceRecapResult
                            {
                                Date = e.Key.Date,
                                Session = e.Key.Session,
                                ClassId = e.Key.ClassId,
                                Homeroom = e.Key.Homeroom
                            }).OrderBy(e => e.Date).Distinct()
                : queryUaEa
                            .GroupBy(e => new
                            {
                                Date = e.Date,
                                Homeroom = e.Homeroom
                            })
                            .Select(e => new GetDataDetailAttendanceRecapResult
                            {
                                Date = e.Key.Date,
                                Homeroom = e.Key.Homeroom
                            }).OrderBy(e => e.Date).Distinct();

            queryUaEa = mappingAttendance.AbsentTerms == AbsentTerm.Session
                        ? queryUaEa.OrderBy(x => x.Date).ThenBy(x => x.Session)
                        : queryUaEa.OrderBy(x => x.Date);

            if (!string.IsNullOrEmpty(param.Search))
            {
                queryUaEa = queryUaEa.Where(x => x.Date.ToString().Contains(param.Search) || x.ClassId.ToLower().Contains(param.Search.ToLower()) || x.Session.ToLower().Contains(param.Search.ToLower()));
            }

            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                switch (param.OrderBy.ToLower())
                {
                    case "date":
                        queryUaEa = queryUaEa = param.OrderType == OrderType.Desc
                            ? queryUaEa.OrderByDescending(x => x.Date)
                            : queryUaEa.OrderBy(x => x.Date);
                        break;
                    case "classid":
                        queryUaEa = param.OrderType == OrderType.Desc
                            ? queryUaEa.OrderByDescending(x => x.ClassId)
                            : queryUaEa.OrderBy(x => x.ClassId);
                        break;
                    case "session":
                        queryUaEa = queryUaEa = param.OrderType == OrderType.Desc
                           ? queryUaEa.OrderByDescending(x => x.Session)
                           : queryUaEa.OrderBy(x => x.Session);
                        break;
                    default:
                        queryUaEa = queryUaEa.OrderByDescending(x => x.Date);
                        break;
                }
            }

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = queryUaEa
                    .ToList();
            }
            else
            {
                items = queryUaEa
                    .SetPagination(param)
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : queryUaEa.Select(x => x.Date).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty());
        }

        public async Task<List<AttendanceEntryNewResult>> GetAttendanceEntryByStudentAsync(string idAcademicYear, string idStudent, DateTime startDate, DateTime endDate,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idAcademicYear))
                throw new InvalidOperationException();

            if (string.IsNullOrWhiteSpace(idStudent))
                throw new InvalidOperationException();

            var queryAttendanceEntry = _dbContext.Entity<TrAttendanceEntryV2>()
                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Session)
                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Subject)
                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Lesson)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                .ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Include(e => e.AttendanceMappingAttendance).ThenInclude(e => e.Attendance)
                .Include(e => e.AttendanceEntryWorkhabitV2s)
                .Where(e => e.ScheduleLesson.IdAcademicYear == idAcademicYear && e.HomeroomStudent.IdStudent == idStudent &&
                            e.ScheduleLesson.ScheduleDate.Date >= startDate && e.ScheduleLesson.ScheduleDate.Date <= endDate &&
                            e.ScheduleLesson.IsGenerated == true && e.Status == AttendanceEntryStatus.Pending);

            var results = await queryAttendanceEntry
                .GroupBy(e => new AttendanceEntryNewResult
                {
                    IdAttendanceEntry = e.IdAttendanceEntry,
                    IdScheduleLesson = e.IdScheduleLesson,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    ScheduleDate = e.ScheduleLesson.ScheduleDate,
                    IdLesson = e.ScheduleLesson.IdLesson,
                    ClassID = e.ScheduleLesson.ClassID,
                    IdGrade = e.ScheduleLesson.IdGrade,
                    IdAcademicYear = e.ScheduleLesson.IdAcademicYear,
                    IdLevel = e.ScheduleLesson.IdLevel,
                    IdStudent = e.HomeroomStudent.IdStudent,
                    Session = new AttendanceSummarySessionResult
                    {
                        Id = e.ScheduleLesson.Session.Id,
                        Name = e.ScheduleLesson.Session.Name,
                        SessionID = e.ScheduleLesson.Session.SessionID.ToString()
                    },
                    Status = e.Status,
                    GradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
                    Classroom = new CodeWithIdVm
                    {
                        Id = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Id,
                        Description = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description,
                        Code = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                    },
                    IdHomeroom = e.HomeroomStudent.Homeroom.Id,
                    Homeroom = e.HomeroomStudent.Homeroom.Grade.Code + e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            return results;
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
