using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.EventAttendanceEntry.Validator;
using BinusSchool.Attendance.FnAttendance.MapAttendance;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry;
using BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using BinusSchool.Common.Model.Enums;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Components.Web;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Wordprocessing;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using DocumentFormat.OpenXml.Math;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry
{
    public class AllPresentExcuseAbsentHandler : FunctionsHttpSingleHandler
    {
        protected IDbContextTransaction Transaction;
        private readonly IAttendanceDbContext _dbContext;
        private readonly GetMapAttendanceDetailHandler _mapAttendanceHandler;
        private readonly IFeatureManagerSnapshot _featureManager;

        public AllPresentExcuseAbsentHandler(
            IAttendanceDbContext dbContext,
            GetMapAttendanceDetailHandler mapAttendanceHandler,
            IFeatureManagerSnapshot featureManager)
        {
            _dbContext = dbContext;
            _mapAttendanceHandler = mapAttendanceHandler;
            _featureManager = featureManager;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<AllPresentAllExcuseEventAttendanceRequest>();

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            if (await _featureManager.IsEnabledAsync(FeatureFlags.AttendanceEventV2))
            {
                var eventType = await _dbContext.Entity<TrEventIntendedForAttendanceStudent>()
                    .Include(x => x.EventIntendedFor)
                        .ThenInclude(x => x.Event)
                    .Where(x => x.EventIntendedFor.Event.Id == body.IdEvent)
                    .FirstOrDefaultAsync();

                if (eventType == null)
                {
                    throw new BadRequestException("Event Not Found !");
                }

                if (eventType.Type == EventIntendedForAttendanceStudent.All || eventType.Type == EventIntendedForAttendanceStudent.Excuse)
                {
                    var checkStudents = await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
                                        .Include(x => x.EventIntendedForAttendanceStudent).ThenInclude(x => x.EventIntendedFor)
                                            .ThenInclude(x => x.Event).ThenInclude(x => x.EventDetails).ThenInclude(x => x.UserEvents)
                                        .Where(x => x.EventIntendedForAttendanceStudent.EventIntendedFor.Event.Id == body.IdEvent)
                                        .ToListAsync(CancellationToken);

                    foreach (var checkStudent in checkStudents)
                    {
                        var userEvents = checkStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails
                                        .SelectMany(x => x.UserEvents).ToList();

                        var GetLevelByUserEvent = await _dbContext.Entity<MsHomeroomStudent>()

                                                .Where(x => userEvents.Select(e => e.IdUser).ToList().Contains(x.IdStudent)
                                                            && x.Homeroom.Semester == 1
                                                            && x.Homeroom.Grade.Level.IdAcademicYear == eventType.EventIntendedFor.Event.IdAcademicYear)
                                                .Select(x => new
                                                {
                                                    x.Homeroom.Grade.IdLevel,
                                                    x.IdStudent
                                                })
                                                .ToListAsync(CancellationToken);

                        var codeAttendance = eventType.Type == EventIntendedForAttendanceStudent.All ? "PR" : "EA";

                        var atdAdministration = await _dbContext.Entity<TrAttendanceAdministration>()
                                                    .Where(x => userEvents.Select(x => x.Id).Contains(x.StudentGrade.IdStudent)
                                                    && (x.StartDate.Date <= checkStudent.StartDate.Date && x.EndDate >= checkStudent.StartDate.Date)
                                                    && (x.StartTime <= checkStudent.StartTime && x.EndTime >= checkStudent.EndTime))
                                                    .Select(x => new
                                                    {
                                                        x.StudentGrade.IdStudent,
                                                        x.IdAttendance
                                                    })
                                                    .ToListAsync(CancellationToken);

                        foreach (var user in userEvents)
                        {
                            var IdLevel = GetLevelByUserEvent.Where(e => e.IdStudent == user.IdUser).Select(e => e.IdLevel).FirstOrDefault();

                            var idMappingAtd = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                                               .Where(x => x.MappingAttendance.IdLevel == IdLevel && x.Attendance.Code == codeAttendance)
                                               .Select(x => x.Id)
                                               .FirstOrDefaultAsync(CancellationToken);

                            var hasConflict = checkStudent.IsPrimary && await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
                                            .Where(x
                                                => x.StartDate.Date == checkStudent.StartDate.Date
                                                && x.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails.Any(y => y.UserEvents.Any(z => z.IdUser == user.Id)))
                                            .AnyAsync(CancellationToken);

                            var idAtdStudent = atdAdministration.Where(x => x.IdStudent == user.IdUser).Select(x => x.IdAttendance).FirstOrDefault();

                            if (!hasConflict && checkStudent.IsPrimary)
                            {
                                var scheduleLessonIds = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                .Include(x => x.GeneratedScheduleStudent)
                                .Where(x
                                    => x.GeneratedScheduleStudent.IdStudent == user.IdUser
                                    && x.ScheduleDate.Date == checkStudent.StartDate.Date
                                    && ((x.StartTime <= checkStudent.StartTime && x.EndTime >= checkStudent.EndTime) || (x.StartTime >= checkStudent.StartTime && x.EndTime <= checkStudent.EndTime)))
                                .Select(x => x.Id)
                                .ToListAsync(CancellationToken);

                                var studentAtdAdministration = atdAdministration.Any(x => x.IdStudent == user.IdUser);

                                foreach (var scheduleId in scheduleLessonIds)
                                {
                                    //overwrite data in attendance entry if exist
                                    var lessonExistInEntry = await _dbContext.Entity<TrAttendanceEntry>()
                                        .Where(x => x.IdGeneratedScheduleLesson == scheduleId)
                                        .ToListAsync(CancellationToken);

                                    foreach (var lesson in lessonExistInEntry)
                                    {
                                        lesson.IsActive = false;
                                    }
                                    _dbContext.Entity<TrAttendanceEntry>().UpdateRange(lessonExistInEntry);

                                    if (studentAtdAdministration)
                                    {
                                        var IdLevelByIdStudent1 = GetLevelByUserEvent.Where(e => e.IdStudent == idAtdStudent).Select(e => e.IdLevel).ToList();

                                        var IdLevelByIdStudent = GetLevelByUserEvent.Where(e => e.IdStudent == idAtdStudent).Select(e => e.IdLevel).FirstOrDefault();
                                        var idMapping = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                                                .Where(x => x.MappingAttendance.IdLevel == IdLevelByIdStudent
                                                && x.Attendance.Code == codeAttendance
                                                && x.IdAttendance == idAtdStudent)
                                                .Select(x => x.Id)
                                                .FirstOrDefaultAsync(CancellationToken);

                                        if (!checkStudent.EventIntendedForAttendanceStudent.IsSetAttendance)
                                        {
                                            var attendanceEntry = new TrAttendanceEntry
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                IdGeneratedScheduleLesson = scheduleId,
                                                IdAttendanceMappingAttendance = idMapping,
                                                LateTime = null,
                                                FileEvidence = null,
                                                Notes = null,
                                                Status = AttendanceEntryStatus.Submitted
                                            };
                                            _dbContext.Entity<TrAttendanceEntry>().Add(attendanceEntry);
                                        }
                                    }
                                    else
                                    {
                                        if (!checkStudent.EventIntendedForAttendanceStudent.IsSetAttendance)
                                        {
                                            var attendanceEntry = new TrAttendanceEntry
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                IdGeneratedScheduleLesson = scheduleId,
                                                IdAttendanceMappingAttendance = idMappingAtd,
                                                LateTime = null,
                                                FileEvidence = null,
                                                Notes = null,
                                                Status = AttendanceEntryStatus.Submitted
                                            };
                                            _dbContext.Entity<TrAttendanceEntry>().Add(attendanceEntry);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    throw new BadRequestException("Type of event not all or excuse");
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
