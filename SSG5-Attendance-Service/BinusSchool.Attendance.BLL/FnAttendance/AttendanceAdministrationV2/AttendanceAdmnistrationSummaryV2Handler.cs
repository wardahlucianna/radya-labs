using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministration.validator;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class AttendanceAdmnistrationSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public AttendanceAdmnistrationSummaryHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetAttendanceAdministrationSummaryRequest, GetAttendanceAdministratorSummaryValidator>();
            var levelStudent = await _dbContext.Entity<MsStudentGrade>()
                       .Include(x => x.Grade)
                           .ThenInclude(x => x.Level)
                       .Where(x => x.IdStudent == body.IdStudent)
                       .Where(x => x.Grade.Level.IdAcademicYear == body.IdAcademicYear)
                       .Select(x => x.Grade.IdLevel).FirstOrDefaultAsync();
            if (levelStudent == null)
                throw new Exception("Student doesnt have level");
            var percentageQuota = await _dbContext.Entity<MsMappingAttendanceQuota>()
                        .Include(x => x.Attendance)
                        .Include(x => x.Level)
                            .ThenInclude(x => x.AcademicYear)
                        .Where(x => x.IdLevel == levelStudent)
                        .Where(x => x.Level.IdAcademicYear == body.IdAcademicYear)
                        .Where(x => x.IdAttendance == body.IdAttendance).FirstOrDefaultAsync();
            if (percentageQuota == null)
            {
                var dataAttendance = await _dbContext.Entity<MsAttendance>()
                .Where(x => x.Id == body.IdAttendance).FirstOrDefaultAsync();
                throw new Exception($"Attendance quota for attendance {dataAttendance?.Description} has not been set");
            }

            var homeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom)
                .Where(x => x.IdStudent == body.IdStudent)
                .Where(x => x.Homeroom.IdAcademicYear == body.IdAcademicYear)
                .Select(x => new { x.IdHomeroom, x.Homeroom.IdGrade })
                .ToListAsync(CancellationToken);

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                .Where(e => homeroomStudent.Select(x=> x.IdGrade).Contains(e.IdGrade) && (e.StartDate.Date <= body.StartDate.Date && e.EndDate.Date >= body.EndDate.Date))
                .Select(e => new { e.Semester,e.StartDate,e.EndDate,
                })
                .Distinct()
                .ToListAsync(CancellationToken);

            //var totalSessionStudent = await _dbContext.Entity<TrGeneratedScheduleLesson>()
            //    .Include(x => x.GeneratedScheduleStudent)
            //        .ThenInclude(x => x.Student)
            //            .ThenInclude(x => x.StudentGrades)
            //                .ThenInclude(x => x.Grade)
            //                    .ThenInclude(x => x.Level)
            //    .OrderBy(x => x.ScheduleDate)
            //    .Where(x => x.GeneratedScheduleStudent.IdStudent == body.IdStudent)
            //    .Where(x => homeroomStudent.Contains(x.IdHomeroom))
            //    .ToListAsync();
            //if (totalSessionStudent.Count == 0)
            //    throw new Exception($"Session for student not found");

            var listStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                .Where(e => homeroomStudent.Select(x => x.IdHomeroom).Contains(e.HomeroomStudent.IdHomeroom)
                            && e.HomeroomStudent.IdStudent == body.IdStudent
                            && homeroomStudent.Select(x => x.IdGrade).Contains(e.HomeroomStudent.Homeroom.IdGrade)
                            )
                .GroupBy(e => new
                {
                    e.IdHomeroomStudent,
                    e.HomeroomStudent.Homeroom.Grade.IdLevel,
                    e.IdLesson
                })
                .Select(e => new
                {
                    e.Key.IdHomeroomStudent,
                    e.Key.IdLevel,
                    e.Key.IdLesson
                })
                .ToListAsync(CancellationToken);

            var totalSessionStudent = await _dbContext.Entity<MsScheduleLesson>()
                    .Where(e => homeroomStudent.Select(x => x.IdGrade).Contains(e.IdGrade)
                            && listStudentEnrollment.Select(x => x.IdLesson).Contains(e.IdLesson)
                            && e.ScheduleDate.Date >= listPeriod.Min().StartDate.Date
                                && e.ScheduleDate.Date <= listPeriod.Max().EndDate.Date)
                    .Select(e => new
                    {
                        e.Id,
                        e.ScheduleDate,
                        e.ClassID,
                        e.IdAcademicYear,
                        e.IdLesson,
                        e.IdDay,
                        e.IdWeek,
                        e.StartTime,
                        e.EndTime,
                        e.SessionID
                    })
                    .ToListAsync(CancellationToken);

            var totalSessionWillUsed = totalSessionStudent
            .Where(x => x.ScheduleDate.Date >= body.StartDate.Date)
            .Where(x => x.ScheduleDate.Date <= body.EndDate.Date);
            if (!body.IsAllDay)
            {
                totalSessionWillUsed = totalSessionWillUsed
                .Where(x => (body.StartPeriod >= x.StartTime && body.StartPeriod <= x.EndTime)
                            || (body.EndPeriod > x.StartTime && body.EndPeriod <= x.EndTime)
                            || (body.StartPeriod <= x.StartTime && body.EndPeriod >= x.EndTime));
            }

            var _totalSessionWillUsed = totalSessionWillUsed.Count();//.GroupBy(x => new { x.ScheduleDate, x.ClassID, x.IdSubject, x.SubjectName, x.IdHomeroom, x.HomeroomName, x.IdSession, x.SessionID }).Count();
            var quota = Math.Ceiling((percentageQuota.Percentage / 100) * totalSessionStudent.Count());

            var currentUsed1 = await _dbContext.Entity<TrAttendanceAdministration>()
                .Include(x => x.StudentGrade)
                        .ThenInclude(x => x.Grade)
                            .ThenInclude(x => x.Level)
                .Where(x => x.StudentGrade.Grade.Level.IdAcademicYear == body.IdAcademicYear)
                .Where(x => x.IdAttendance == body.IdAttendance)
                .Where(x => x.StudentGrade.IdStudent == body.IdStudent)
                .GroupBy(x => new
                {
                    x.Id,
                    x.IdAttendance,
                    x.StudentGrade.IdStudent,
                    x.SessionUsed
                }).Select(e=>e.Key).ToListAsync(CancellationToken);

            var getDataAttendanceAdministration = await _dbContext.Entity<TrAttendanceAdministration>()
                .Include(x => x.StudentGrade)
                        .ThenInclude(x => x.Grade)
                            .ThenInclude(x => x.Level)
                .Where(x => x.StudentGrade.Grade.Level.IdAcademicYear == body.IdAcademicYear)
                .Where(x => x.IdAttendance == body.IdAttendance)
                .Where(x => x.StudentGrade.IdStudent == body.IdStudent)
                .ToListAsync(CancellationToken);

            var groupedData = getDataAttendanceAdministration.GroupBy(x => new
            {
                x.Id,
                x.IdAttendance,
                x.StudentGrade.IdStudent,
                x.SessionUsed
            }).ToList();

            var currentUsed = groupedData.Sum(x => x.Key.SessionUsed);

            var result = new GetAttendanceAdministrationSummaryResult
            {
                Quota = new QuotaVm
                {
                    TotalQuotaSession = (int)quota,
                    TotalStudentSession = totalSessionStudent.Count,
                },
                Used = new Use
                {
                    TotalUse = currentUsed,
                    PercentageUse = decimal.Round((currentUsed / quota) * 100, 2)
                },
                WillUsed = _totalSessionWillUsed,
                RemainingAfterUsed = new RemainingAfterUsed
                {
                    TotalRemainingAfterUsed = (int)quota - (_totalSessionWillUsed + currentUsed),
                    PercentageRemainingAfterUsed = decimal.Round(((quota - (_totalSessionWillUsed + currentUsed)) / quota) * 100, 2)
                }
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
