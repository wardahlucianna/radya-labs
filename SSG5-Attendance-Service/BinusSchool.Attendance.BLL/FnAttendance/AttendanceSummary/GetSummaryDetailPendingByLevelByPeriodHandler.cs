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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryDetailPendingByLevelByPeriodHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetSummaryDetailPendingByLevelByPeriodHandler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = Request.ValidateParams<GetSummaryDetailUnsubmittedByLevelByPeriodRequest>(
                nameof(GetSummaryDetailUnsubmittedByLevelByPeriodRequest.IdHomeroom)
                );

            var queryBasic = _dbContext.Entity<TrGeneratedScheduleLesson>()
                                            .Include(x => x.GeneratedScheduleStudent)
                                            .Include(x => x.AttendanceEntries)
                                            .Include(x => x.Homeroom.Grade)
                                            .Where(x => x.Homeroom.Semester == param.Semester)
                                            .Where(x => x.IsGenerated && x.IdHomeroom == param.IdHomeroom)
                                            .Where(x => x.AttendanceEntries.Any(y => y.Status == AttendanceEntryStatus.Pending))
                                            .GroupBy(x => new
                                            {
                                                x.ClassID,
                                                x.ScheduleDate,
                                                x.SubjectName,
                                                x.TeacherName,
                                                x.IdSession,
                                                x.SessionID,
                                                x.IdHomeroom,
                                                x.HomeroomName,
                                                x.IdSubject,
                                                x.IdUser
                                            }
                                            );
            if (param.OrderType == OrderType.Asc)
            {
                switch (param.OrderBy)
                {
                    case "date":
                        queryBasic = queryBasic.OrderBy(x => x.Key.ScheduleDate);
                        break;
                    case "classId":
                        queryBasic = queryBasic.OrderBy(x => x.Key.ClassID);
                        break;
                    case "teacher":
                        queryBasic = queryBasic.OrderBy(x => x.Key.TeacherName);
                        break;
                    case "subject":
                        queryBasic = queryBasic.OrderBy(x => x.Key.SubjectName);
                        break;
                    case "session":
                        queryBasic = queryBasic.OrderBy(x => x.Key.SessionID);
                        break;
                    default:
                        queryBasic = queryBasic.OrderBy(x => x.Key.ScheduleDate);
                        break;
                };

            }
            else
            {
                switch (param.OrderBy)
                {
                    case "date":
                        queryBasic = queryBasic.OrderByDescending(x => x.Key.ScheduleDate);
                        break;
                    case "classId":
                        queryBasic = queryBasic.OrderByDescending(x => x.Key.ClassID);
                        break;
                    case "teacher":
                        queryBasic = queryBasic.OrderByDescending(x => x.Key.TeacherName);
                        break;
                    case "subject":
                        queryBasic = queryBasic.OrderByDescending(x => x.Key.SubjectName);
                        break;
                    case "session":
                        queryBasic = queryBasic.OrderByDescending(x => x.Key.SessionID);
                        break;
                    default:
                        queryBasic = queryBasic.OrderByDescending(x => x.Key.ScheduleDate);
                        break;
                };
            }
            var columns = new[] { "date", "classId", "teacher", "subject", "session" };
            var queryData = await queryBasic.SetPagination(param)
                .Select(x=> new GetSummaryDetailUnsubmittedByLevelByPeriodResponse
                {
                    ScheduleDate = x.Key.ScheduleDate,
                    ClassId = x.Key.ClassID,
                    SessionNo = x.Key.SessionID,
                    SubjectId = new ItemValueVm
                    {
                        Id = x.Key.IdSubject,
                        Description = x.Key.SubjectName
                    },
                    Teacher = new ItemValueVm
                    {
                        Id = x.Key.IdUser,
                        Description = x.Key.TeacherName
                    },
                    Homeroom = new ItemValueVm
                    {
                        Id = x.Key.IdHomeroom,
                        Description = x.Key.HomeroomName
                    },
                    TotalAttendance = x.Count()
                })
                .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(queryData.Count)
            ? queryData.Count
            : await queryBasic.Select(x => x.Key.ScheduleDate).CountAsync(CancellationToken);
            return Request.CreateApiResult2(queryData as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
