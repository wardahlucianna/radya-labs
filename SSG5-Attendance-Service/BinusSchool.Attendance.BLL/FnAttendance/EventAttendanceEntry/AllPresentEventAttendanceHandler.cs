using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry
{
    public class AllPresentEventAttendanceHandler : FunctionsHttpSingleHandler
    {
        protected IDbContextTransaction Transaction;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IFeatureManagerSnapshot _featureManager;

        public AllPresentEventAttendanceHandler(
            IAttendanceDbContext dbContext, IFeatureManagerSnapshot featureManager)
        {
            _dbContext = dbContext;
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<AllPresentEventAttendanceRequest>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            if (await _featureManager.IsEnabledAsync(FeatureFlags.AttendanceEventV2))
            {
                var attendanceStudents = await _dbContext.Entity<TrEventIntendedFor>()
                    .Include(x => x.Event).ThenInclude(x => x.EventDetails).ThenInclude(x => x.UserEvents).ThenInclude(x => x.UserEventAttendance2s)
                    .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdCheckStudents)
                    .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedFor).ThenInclude(x => x.Event).ThenInclude(x => x.EventDetails).ThenInclude(x => x.UserEvents).ThenInclude(x => x.UserEventAttendance2s)
                    .Where(x 
                        => x.IdEvent == body.IdEvent
                        && x.IntendedFor == RoleConstant.Student
                        && x.EventIntendedForAttendanceStudents.Any(y 
                            => y.IsSetAttendance 
                            && y.EventIntendedForAtdPICStudents.Any(z => z.IdUser == AuthInfo.UserId)))
                    .SelectMany(x => x.EventIntendedForAttendanceStudents)
                    .ToListAsync(CancellationToken);
                
                if (attendanceStudents.Count == 0)
                    throw new NotFoundException("Event is not found or you're not PIC of this event");
                
                var presentAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>().Include(x => x.MappingAttendance).ThenInclude(x => x.Level)
                    .Where(x => x.Attendance.AttendanceCategory == AttendanceCategory.Present && x.Attendance.Code == "PR")
                    .ToListAsync(CancellationToken);
                
                foreach (var checkStudent in attendanceStudents.SelectMany(x => x.EventIntendedForAtdCheckStudents))
                {
                    var userEvents = checkStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails
                        .SelectMany(x => x.UserEvents).Where(x => !x.UserEventAttendance2s.Any(y => y.IdEventIntendedForAtdCheckStudent == checkStudent.Id));

                    var idUsers = userEvents.Select(x => x.IdUser);
                    var students = await _dbContext.Entity<MsStudent>()
                        .Where(x => idUsers.Contains(x.Id))
                        .Select(x => new { x.Id, x.StudentGrades.FirstOrDefault().Grade.Level })
                        .ToListAsync(CancellationToken);

                    foreach (var userEvent in userEvents)
                    {
                        var student = students.Where(x => x.Id == userEvent.IdUser);
                        var mapping = presentAttendance.Find(x => x.MappingAttendance.IdLevel == student.FirstOrDefault().Level.Id);

                        if (mapping is null)
                            throw new BadRequestException($"Present attendance on level {student.FirstOrDefault().Level.Description} is not mapped yet");

                        if (student is {})
                        {
                            var hasConflict = checkStudent.IsPrimary && await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
                                .Where(x 
                                    => x.StartDate.Date == checkStudent.StartDate.Date
                                    && x.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails.Any(y => y.UserEvents.Any(z => z.IdUser == userEvent.Id)))
                                .AnyAsync(CancellationToken);
                            
                            var userEventAttendance = new TrUserEventAttendance2
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdUserEvent = userEvent.Id,
                                IdEventIntendedForAtdCheckStudent = checkStudent.Id,
                                IdAttendanceMappingAttendance = mapping.Id,
                                DateEvent = checkStudent.StartDate,
                                HasBeenResolved = !hasConflict
                            };
                            _dbContext.Entity<TrUserEventAttendance2>().Add(userEventAttendance);

                            // action for replace attendance entry
                            if (!hasConflict && checkStudent.IsPrimary)
                            {
                                var scheduleLessonIds = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                    .Include(x => x.GeneratedScheduleStudent)
                                    .Where(x 
                                        => x.GeneratedScheduleStudent.IdStudent == userEvent.IdUser
                                        && x.ScheduleDate.Date == checkStudent.StartDate.Date
                                        && ((x.StartTime <= checkStudent.StartTime && x.EndTime >= checkStudent.StartTime) || (x.StartTime <= checkStudent.EndTime && x.EndTime >= checkStudent.EndTime)))
                                    .Select(x => x.Id)
                                    .ToListAsync(CancellationToken);
                                
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

                                    var attendanceEntry = new TrAttendanceEntry
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdGeneratedScheduleLesson = scheduleId,
                                        IdAttendanceMappingAttendance = mapping.Id
                                    };
                                    _dbContext.Entity<TrAttendanceEntry>().Add(attendanceEntry);
                                }
                            }
                        }
                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await Transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            Transaction?.Rollback();
            var response = Request.CreateApiErrorResponse(ex);

            return Task.FromResult(response as IActionResult);
        }

        protected override void OnFinally()
        {
            Transaction?.Dispose();
        }
    }
}
