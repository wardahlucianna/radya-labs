using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryDetailUnsubmittedByLevelByRangeHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetSummaryDetailUnsubmittedByLevelByRangeHandler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSummaryDetailUnsubmittedByLevelByRangeRequest>(
               nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.IdHomeroom),
               nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.StartDate),
               nameof(GetSummaryDetailUnsubmittedByLevelByRangeRequest.EndDate)
               );

            var query2 = _dbContext.Entity<TrGeneratedScheduleLesson>()
                                            .Include(x => x.GeneratedScheduleStudent)
                                            .Include(x => x.AttendanceEntries)
                                            .Include(x => x.Homeroom.Grade)
                                            .Where(x => x.IsGenerated
                                            && x.IdHomeroom == param.IdHomeroom
                                            && x.ScheduleDate.Date >= param.StartDate
                                            && x.ScheduleDate.Date <= param.EndDate
                                            && x.ScheduleDate.Date <= _dateTime.ServerTime.Date)
                                            .Where(x => !x.AttendanceEntries.Any())
                                            .ToList()
                                            .GroupBy(x => new
                                            {
                                                x.ClassID,
                                                x.ScheduleDate,
                                                x.SubjectName,
                                                //x.TeacherName,
                                                x.IdSession,
                                                x.SessionID,
                                                x.IdSubject,
                                                //x.IdUser
                                            }
                                            );
            
            var queryBasic = query2.Select(x => new
            {
                ClassID = x.Key.ClassID,
                ScheduleDate = x.Key.ScheduleDate,
                SubjectName = x.Key.SubjectName,
                TeacherName = string.Join(",", x.Select(x => x.TeacherName).Distinct()),
                IdSession = x.Key.IdSession,
                SessionID = x.Key.SessionID,
                IdSubject = x.Key.IdSubject,
                IdUser = string.Join(",", x.Select(x => x.IdUser).Distinct()),
                Total = (x.Count() / x.Select(x => x.TeacherName).Distinct().Count())
            }).AsQueryable();

            if (param.OrderType == OrderType.Asc)
            {
                switch (param.OrderBy)
                {
                    case "date":
                        queryBasic = queryBasic.OrderBy(x => x.ScheduleDate);
                        break;
                    case "classId":
                        queryBasic = queryBasic.OrderBy(x => x.ClassID);
                        break;
                    case "teacher":
                        queryBasic = queryBasic.OrderBy(x => x.TeacherName);
                        break;
                    case "subject":
                        queryBasic = queryBasic.OrderBy(x => x.SubjectName);
                        break;
                    case "session":
                        queryBasic = queryBasic.OrderBy(x => x.SessionID);
                        break;
                    default:
                        queryBasic = queryBasic.OrderBy(x => x.ScheduleDate);
                        break;
                };

            }
            else
            {
                switch (param.OrderBy)
                {
                    case "date":
                        queryBasic = queryBasic.OrderByDescending(x => x.ScheduleDate);
                        break;
                    case "classId":
                        queryBasic = queryBasic.OrderByDescending(x => x.ClassID);
                        break;
                    case "teacher":
                        queryBasic = queryBasic.OrderByDescending(x => x.TeacherName);
                        break;
                    case "subject":
                        queryBasic = queryBasic.OrderByDescending(x => x.SubjectName);
                        break;
                    case "session":
                        queryBasic = queryBasic.OrderByDescending(x => x.SessionID);
                        break;
                    default:
                        queryBasic = queryBasic.OrderByDescending(x => x.ScheduleDate);
                        break;
                };
            }

            var columns = new[] { "date", "classId", "teacher", "subject", "session" };

            var queryData = queryBasic.AsNoTracking().AsEnumerable().SetPagination(param)
                .Select(x => new GetSummaryDetailUnsubmittedByLevelByPeriodResponse
                {
                    ScheduleDate = x.ScheduleDate,
                    ClassId = x.ClassID,
                    SessionNo = x.SessionID,
                    SubjectId = new ItemValueVm
                    {
                        Id = x.IdSubject,
                        Description = x.SubjectName
                    },
                    Teacher = new ItemValueVm
                    {
                        Id = x.IdUser,
                        Description = x.TeacherName
                    },
                    TotalAttendance = x.Total
                }).ToList();

            var count = param.CanCountWithoutFetchDb(queryData.Count)
            ? queryData.Count
            : await queryBasic.Select(x => x.ScheduleDate).CountAsync(CancellationToken);
            return Request.CreateApiResult2(queryData as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
