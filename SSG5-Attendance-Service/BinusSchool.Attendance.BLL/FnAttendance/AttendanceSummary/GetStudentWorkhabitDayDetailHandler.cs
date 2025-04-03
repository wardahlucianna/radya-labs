﻿using System.Linq;
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
    public class GetStudentWorkhabitDayDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetStudentWorkhabitDayDetailHandler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentWorkhabitDetailRequest>(nameof(GetStudentWorkhabitDetailRequest.IdSchool),
                                                                                 nameof(GetStudentWorkhabitDetailRequest.IdStudent),
                                                                                 nameof(GetStudentWorkhabitDetailRequest.IdWorkhabit));

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

            var homeroomTeacher = currentStudentHomeroom.HomeroomTeachers.Where(x => x.IsAttendance)
                                                                         .Select(x => NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName))
                                                                         .FirstOrDefault();

            var query = _dbContext.Entity<TrAttendanceEntryV2>()
                                    .Include(x => x.HomeroomStudent)
                                    .Include(x => x.AttendanceEntryWorkhabitV2s)
                                        .ThenInclude(x => x.MappingAttendanceWorkhabit)
                                    .Include(x => x.ScheduleLesson)
                                        .ThenInclude(x => x.Session)
                                    .Where(x => x.Status == AttendanceEntryStatus.Submitted
                                                && x.HomeroomStudent.IdStudent == param.IdStudent
                                                && x.AttendanceEntryWorkhabitV2s.Any(y => y.MappingAttendanceWorkhabit.Id == param.IdWorkhabit))
                                    .GroupBy(x => new { x.ScheduleLesson.ScheduleDate.Date, x.Notes })
                                    .Select(x => new
                                    {
                                        Date = x.Key.Date,
                                        TeacherName = homeroomTeacher,
                                        Comment = x.Key.Notes
                                    });

            //var query = _dbContext.Entity<TrAttendanceEntry>()
            //                      .Include(x => x.GeneratedScheduleLesson)
            //                            .ThenInclude(x => x.GeneratedScheduleStudent)
            //                      .Include(x => x.AttendanceEntryWorkhabits)
            //                            .ThenInclude(x => x.MappingAttendanceWorkhabit)
            //                      .Where(x => x.GeneratedScheduleLesson.IsGenerated
            //                                  && x.GeneratedScheduleLesson.IdHomeroom == currentStudentHomeroom.Id
            //                                  && x.GeneratedScheduleLesson.GeneratedScheduleStudent.IdStudent == param.IdStudent
            //                                  && x.Status == AttendanceEntryStatus.Submitted
            //                                  && x.AttendanceEntryWorkhabits.Any(y => y.MappingAttendanceWorkhabit.Id == param.IdWorkhabit))
            //                      .GroupBy(x => new { x.GeneratedScheduleLesson.ScheduleDate.Date, x.Notes })
            //                      .Select(x => new GetStudentWorkhabitDayDetailResult
            //                      {
            //                          Date = x.Key.Date,
            //                          TeacherName = homeroomTeacher,
            //                          Comment = x.Key.Notes
            //                      });

            switch (param.OrderBy)
            {
                case "date":
                    query = param.OrderType == OrderType.Asc ?
                            query.OrderBy(x => x.Date) :
                            query.OrderByDescending(x => x.Date);
                    break;
                case "teacher":
                    query = param.OrderType == OrderType.Asc ?
                            query.OrderBy(x => x.TeacherName) :
                            query.OrderByDescending(x => x.TeacherName);
                    break;
                case "comment":
                    query = param.OrderType == OrderType.Asc ?
                            query.OrderBy(x => x.Comment) :
                            query.OrderByDescending(x => x.Comment);
                    break;
                default:
                    query = param.OrderType == OrderType.Asc ?
                            query.OrderBy(x => x.Date) :
                            query.OrderByDescending(x => x.Date);
                    break;
            };
            var columns = new[] { "date", "teacher", "comment" };
            var res = await query.SetPagination(param).ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(res.Count)
            ? res.Count
          : await query.CountAsync(CancellationToken);
            return Request.CreateApiResult2(res as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
