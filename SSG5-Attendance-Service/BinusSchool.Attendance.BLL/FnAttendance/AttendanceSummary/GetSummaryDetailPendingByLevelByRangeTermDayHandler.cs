using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryDetailPendingByLevelByRangeTermDayHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetSummaryDetailPendingByLevelByRangeTermDayHandler(
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

            var queryBasic = _dbContext.Entity<TrGeneratedScheduleLesson>()
                                            .Include(x => x.GeneratedScheduleStudent)
                                            .Include(x => x.AttendanceEntries)
                                            .Include(x => x.Homeroom.Grade)
                                            .Where(x => x.IsGenerated
                                            && x.IdHomeroom == param.IdHomeroom
                                            && x.ScheduleDate.Date >= param.StartDate
                                            && x.ScheduleDate.Date <= param.EndDate)
                                            .Where(x => x.AttendanceEntries.Any(y => y.Status == AttendanceEntryStatus.Pending))
                                            .GroupBy(x => new
                                            {
                                                x.ScheduleDate,
                                                x.IdHomeroom,
                                                x.HomeroomName
                                            }
                                            );
            if (param.OrderType == OrderType.Asc)
            {
                switch (param.OrderBy)
                {
                    case "date":
                        queryBasic = queryBasic.OrderBy(x => x.Key.ScheduleDate);
                        break;
                    case "homeroom":
                        queryBasic = queryBasic.OrderBy(x => x.Key.HomeroomName);
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
                        queryBasic = queryBasic.OrderBy(x => x.Key.ScheduleDate);
                        break;
                    case "homeroom":
                        queryBasic = queryBasic.OrderBy(x => x.Key.HomeroomName);
                        break;
                    default:
                        queryBasic = queryBasic.OrderBy(x => x.Key.ScheduleDate);
                        break;
                };
            }
            var columns = new[] { "date", "homeroom"};
            var queryData = await queryBasic.SetPagination(param)
                            .Select(x => new GetSummaryDetailUnsubmittedByLevelByPeriodTermDayResponse
                            {
                                ScheduleDate = x.Key.ScheduleDate,
                                Homeroom = new ItemValueVm
                                {
                                    Id = x.Key.IdHomeroom,
                                    Description = x.Key.HomeroomName
                                },
                                TotalAttendance = _dbContext.Entity<TrGeneratedScheduleLesson>()
                                .Include(y => y.GeneratedScheduleStudent)
                                .Where(y => y.IdHomeroom == x.Key.IdHomeroom && y.HomeroomName == x.Key.HomeroomName)
                                .GroupBy(y => y.GeneratedScheduleStudent.IdStudent)
                                .Select(x => x.Key)
                                .Count()
                            })
                            .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(queryData.Count)
            ? queryData.Count
            : await queryBasic.Select(x => x.Key.ScheduleDate).CountAsync(CancellationToken);

            var homeroomIds = queryData.Select(x => x.Homeroom.Id).Distinct();
            var homeroomTeachers = await _dbContext.Entity<MsHomeroomTeacher>()
                                            .Include(x => x.Staff)
                                            .Include(x => x.TeacherPosition)
                                                .ThenInclude(x => x.LtPosition)
                                            .Where(x => homeroomIds.Contains(x.IdHomeroom)
                                                       && x.TeacherPosition.LtPosition.Code == PositionConstant.ClassAdvisor)
                                            .Select(x => new
                                            {
                                                x.IdHomeroom,
                                                x.IdBinusian,
                                                Name = $"{x.Staff.FirstName} {x.Staff.LastName}"
                                            })
                                            .ToListAsync(CancellationToken);

            var resultData = queryData.Select(x => new GetSummaryDetailUnsubmittedByLevelByPeriodTermDayResponse
            {
                ScheduleDate = x.ScheduleDate,
                Homeroom = x.Homeroom,
                Teacher = homeroomTeachers.Where(y => y.IdHomeroom == x.Homeroom.Id).Select(y => new ItemValueVm
                {
                    Id = y.IdBinusian,
                    Description = y.Name
                }).FirstOrDefault(),
                TotalAttendance = x.TotalAttendance
            }).ToList();

            return Request.CreateApiResult2(resultData as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
