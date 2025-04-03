using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetStudentAttendanceSummarySessionDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetStudentAttendanceSummarySessionDetailHandler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentAttendanceSummaryDetailRequest>(nameof(GetStudentAttendanceSummaryDetailRequest.IdSchool),
                                                                                         nameof(GetStudentAttendanceSummaryDetailRequest.IdStudent));

            var currentAcademic = await _dbContext.Entity<MsPeriod>()
                                                  .Include(x => x.Grade)
                                                       .ThenInclude(x => x.Level)
                                                           .ThenInclude(x => x.AcademicYear)
                                                  .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool
                                                              && _dateTime.ServerTime.Date >= x.StartDate.Date
                                                              && _dateTime.ServerTime.Date <= x.EndDate.Date)
                                                  .Select(x => new CurrentAcademicYearResult
                                                  {
                                                      Id = x.Grade.Level.AcademicYear.Id,
                                                      Code = x.Grade.Level.AcademicYear.Code,
                                                      Description = x.Grade.Level.AcademicYear.Description,
                                                      Semester = x.Semester
                                                  }).FirstOrDefaultAsync();
            if (currentAcademic is null)
                throw new NotFoundException("Current academic year is not defined");

            var currentStudentHomeroom = await _dbContext.Entity<MsHomeroomStudent>()
                                                         .Include(x => x.Homeroom)
                                                              .ThenInclude(x => x.HomeroomTeachers)
                                                                    .ThenInclude(x => x.Staff)
                                                         .Where(x => x.IdStudent == param.IdStudent
                                                                     && x.Homeroom.IdAcademicYear == currentAcademic.Id
                                                                     && x.Homeroom.Semester == currentAcademic.Semester)
                                                         .Select(x => x.Homeroom)
                                                         .FirstOrDefaultAsync(CancellationToken);
            if (currentStudentHomeroom is null)
                throw new NotFoundException("Student's current homeroom is not defined");

            var query = _dbContext.Entity<TrAttendanceEntry>()
                                  .Include(x => x.GeneratedScheduleLesson)
                                        .ThenInclude(x => x.GeneratedScheduleStudent)
                                  .Include(x => x.AttendanceMappingAttendance)
                                  .Where(x => x.GeneratedScheduleLesson.IsGenerated
                                              && x.GeneratedScheduleLesson.IdHomeroom == currentStudentHomeroom.Id
                                              && x.GeneratedScheduleLesson.GeneratedScheduleStudent.IdStudent == param.IdStudent
                                              && x.Status == AttendanceEntryStatus.Submitted
                                              && (string.IsNullOrEmpty(param.IdAttendance) || x.AttendanceMappingAttendance.IdAttendance == param.IdAttendance))
                                  .Select(x => new GetStudentAttendanceSummarySessionDetailResult
                                   {
                                       Date = x.GeneratedScheduleLesson.ScheduleDate,
                                       Session = x.GeneratedScheduleLesson.SessionID,
                                       SubjectName = x.GeneratedScheduleLesson.SubjectName,
                                       TeacherName = x.GeneratedScheduleLesson.TeacherName,
                                       AttendanceName = x.AttendanceMappingAttendance.Attendance.Description,
                                       Reason = x.Notes
                                   });
            var list = query.ToList();
            switch (param.OrderBy)
            {
                case "date":
                    query = param.OrderType == OrderType.Asc ?
                            query.OrderBy(x => x.Date) :
                            query.OrderByDescending(x => x.Date);
                    break;
                case "session":
                    query = param.OrderType == OrderType.Asc ?
                            query.OrderBy(x => x.Session) :
                            query.OrderByDescending(x => x.Session);
                    break;
                case "subject":
                    query = param.OrderType == OrderType.Asc ?
                            query.OrderBy(x => x.SubjectName) :
                            query.OrderByDescending(x => x.SubjectName);
                    break;
                case "teacher":
                    query = param.OrderType == OrderType.Asc ?
                            query.OrderBy(x => x.TeacherName) :
                            query.OrderByDescending(x => x.TeacherName);
                    break;
                case "attendance":
                    query = param.OrderType == OrderType.Asc ?
                            query.OrderBy(x => x.AttendanceName) :
                            query.OrderByDescending(x => x.AttendanceName);
                    break;
                case "reason":
                    query = param.OrderType == OrderType.Asc ?
                            query.OrderBy(x => x.Reason) :
                            query.OrderByDescending(x => x.Reason);
                    break;
                default:
                    query = param.OrderType == OrderType.Asc ?
                            query.OrderBy(x => x.Date) :
                            query.OrderByDescending(x => x.Date);
                    break;
            };
            var columns = new[] { "date", "session", "subject", "teacher", "attendance", "reason" };
            var res = await query.SetPagination(param).ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(res.Count)
            ? res.Count
          : await query.CountAsync(CancellationToken);
            return Request.CreateApiResult2(res as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
