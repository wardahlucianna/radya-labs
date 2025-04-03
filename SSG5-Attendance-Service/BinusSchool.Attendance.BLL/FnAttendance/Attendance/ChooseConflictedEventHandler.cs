using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.EventAttendanceEntry.Validator;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnAttendance.Attendance
{
    public class ChooseConflictedEventHandler : FunctionsHttpSingleHandler
    {
        protected IDbContextTransaction Transaction;
        private readonly IAttendanceDbContext _dbContext;

        public ChooseConflictedEventHandler(
            IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<ChooseConflictedEventRequest, ChooseConflictedEventValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var conflictedIds = body.ConflictCode.Split('_').ToList();

            var conflicteds = await _dbContext.Entity<TrUserEventAttendance>()
                                              .Include(x => x.UserEvent)
                                              .Include(x => x.UserEventAttendanceWorkhabits)
                                              .Include(x => x.EventIntendedForAttendanceCheckStudent)
                                              .Where(x => conflictedIds.Contains(x.Id)
                                                          && !x.HasBeenResolved
                                                          && !x.HasBeenChoose
                                                          && x.EventIntendedForAttendanceCheckStudent.IsPrimary)
                                              .ToListAsync();
            if (conflicteds.Count != conflictedIds.Count)
                throw new BadRequestException("Conflict is not recognize");
            if (!conflicteds.Any(x => x.IdEventIntendedForAtdCheckStudent == body.IdEventCheck))
                throw new BadRequestException("Chosen attendance check is not recognize");

            foreach (var conflict in conflicteds)
            {
                conflict.HasBeenResolved = true;
                conflict.HasBeenChoose = conflict.IdEventIntendedForAtdCheckStudent == body.IdEventCheck;
                _dbContext.Entity<TrUserEventAttendance>().Update(conflict);

                if (conflict.IdEventIntendedForAtdCheckStudent == body.IdEventCheck)
                {
                    // action for replace attendance entry
                    var scheduleLessonIds = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                                            .Include(x => x.GeneratedScheduleStudent)
                                                            .Where(x => x.GeneratedScheduleStudent.IdStudent == conflict.UserEvent.IdUser
                                                                        && x.ScheduleDate.Date == conflict.EventIntendedForAttendanceCheckStudent.StartDate.Date
                                                                        && ((x.StartTime <= conflict.EventIntendedForAttendanceCheckStudent.StartTime && x.EndTime >= conflict.EventIntendedForAttendanceCheckStudent.StartTime)
                                                                            || (x.StartTime <= conflict.EventIntendedForAttendanceCheckStudent.EndTime && x.EndTime >= conflict.EventIntendedForAttendanceCheckStudent.EndTime)))
                                                                .Select(x => x.Id)
                                                                .ToListAsync();
                    foreach (var scheduleId in scheduleLessonIds)
                    {
                        //overwrite data in attendance entry if exist
                        var lessonExistInEntry = await _dbContext.Entity<TrAttendanceEntry>().Where(x => x.IdGeneratedScheduleLesson == scheduleId).ToListAsync();
                        foreach (var lesson in lessonExistInEntry)
                        {
                            lesson.IsActive = false;
                        }
                        _dbContext.Entity<TrAttendanceEntry>().UpdateRange(lessonExistInEntry);

                        var idAttendance = Guid.NewGuid().ToString();
                        _dbContext.Entity<TrAttendanceEntry>().Add(new TrAttendanceEntry
                        {
                            Id = idAttendance,
                            IdGeneratedScheduleLesson = scheduleId,
                            IdAttendanceMappingAttendance = conflict.IdAttendanceMappingAttendance,
                            LateTime = conflict.LateTime,
                            FileEvidence = conflict.FileEvidence,
                            Notes = conflict.Notes
                        });

                        foreach (var studentWorkhabit in conflict.UserEventAttendanceWorkhabits.Select(x => x.IdMappingAttendanceWorkhabit))
                        {
                            _dbContext.Entity<TrAttendanceEntryWorkhabit>().Add(new TrAttendanceEntryWorkhabit
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdAttendanceEntry = idAttendance,
                                IdMappingAttendanceWorkhabit = studentWorkhabit,
                            });
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

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
