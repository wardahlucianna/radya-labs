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
using System.Collections.Generic;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry
{
    public class UpdateEventAttendanceEntryHandler : FunctionsHttpSingleHandler
    {
        private GetMapAttendanceDetailResult _mapAttendance;

        protected IDbContextTransaction Transaction;
        private readonly IAttendanceDbContext _dbContext;
        private readonly GetMapAttendanceDetailHandler _mapAttendanceHandler;
        private readonly IFeatureManagerSnapshot _featureManager;

        public UpdateEventAttendanceEntryHandler(
            IAttendanceDbContext dbContext,
            GetMapAttendanceDetailHandler mapAttendanceHandler,
            IFeatureManagerSnapshot featureManager)
        {
            _dbContext = dbContext;
            _mapAttendanceHandler = mapAttendanceHandler;
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<UpdateEventAttendanceEntryRequest>();

            _mapAttendance = await _mapAttendanceHandler.GetMapAttendanceDetail(body.IdLevel, CancellationToken);
            if (_mapAttendance is null)
            {
                var level = await _dbContext.Entity<MsLevel>().Where(x => x.Id == body.IdLevel).Select(x => x.Description).SingleOrDefaultAsync(CancellationToken);
                throw new BadRequestException($"Mapping attendance for level {level ?? body.IdLevel} is not available.");
            }

            (await new UpdateEventAttendanceEntryValidator(_mapAttendance).ValidateAsync(body)).EnsureValid();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            if (await _featureManager.IsEnabledAsync(FeatureFlags.AttendanceEventV2))
            {
                var checkStudent = await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
                    .Include(x => x.EventIntendedForAttendanceStudent).ThenInclude(x => x.EventIntendedForAtdPICStudents)
                    .Include(x => x.EventIntendedForAttendanceStudent).ThenInclude(x => x.EventIntendedFor)
                        .ThenInclude(x => x.Event).ThenInclude(x => x.EventDetails).ThenInclude(x => x.UserEvents).ThenInclude(x => x.UserEventAttendance2s)
                    .Where(x => x.Id == body.IdEventCheck)
                    .FirstOrDefaultAsync(CancellationToken);

                var eventType = checkStudent.EventIntendedForAttendanceStudent.Type;

                if (checkStudent is null)
                    throw new BadRequestException("Event check is not found");
                if (!checkStudent.EventIntendedForAttendanceStudent.EventIntendedForAtdPICStudents.Any(x => x.IdUser == AuthInfo.UserId))
                    throw new BadRequestException("You're not PIC of this event");

                List<string> IdEventIntendedForAtdCheckStudent = new List<string>();
                var IsRepeat = checkStudent.EventIntendedForAttendanceStudent.IsRepeat;
                if (!IsRepeat)
                {
                    IdEventIntendedForAtdCheckStudent = await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
                    .Where(x => x.EventIntendedForAttendanceStudent.EventIntendedFor.Event.Id == checkStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.Id && x.CheckName== checkStudent.CheckName)
                    .Select(e=>e.Id)
                    .ToListAsync(CancellationToken);
                }
                else
                {
                    IdEventIntendedForAtdCheckStudent.Add(checkStudent.Id);
                }

                var StartDate = checkStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails.Select(e => e.StartDate).FirstOrDefault();
                var EndDate = checkStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails.Select(e => e.EndDate).FirstOrDefault();

                var GetUserEventAttendance2 = await _dbContext.Entity<TrUserEventAttendance2>()
                                                .Where(x => IdEventIntendedForAtdCheckStudent.Contains(x.IdEventIntendedForAtdCheckStudent))
                                                .ToListAsync(CancellationToken);

                var idUserEvents2 = body.Entries.Select(x => x.IdUserEvent);
                var userEvents2 = checkStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails
                    .SelectMany(x => x.UserEvents)
                    .Where(x => idUserEvents2.Contains(x.Id));
                
                // throw when any user event that not found
                var notFoundUserEvents2 = idUserEvents2.Except(userEvents2.Select(x => x.Id)).ToArray();
                if (notFoundUserEvents2.Length != 0)
                    throw new BadRequestException($"Some user event is not found: {string.Join(", ", notFoundUserEvents2)}");

                // make sure no multiple student from requested entries
                var duplicateEntries2 = userEvents2.GroupBy(x => x.IdUser).Where(x => x.Count() > 1)
                    .Select(x => x.First().User.DisplayName)
                    .ToArray();
                if (duplicateEntries2.Length != 0)
                    throw new BadRequestException($"You entered multiple entry for student(s) {string.Join(", ", duplicateEntries2)}.");

                foreach (var item in userEvents2)
                {
                    var hasConflict = checkStudent.IsPrimary && await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
                        .Where(x
                            => x.StartDate.Date == checkStudent.StartDate.Date
                            && x.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails.Any(y => y.UserEvents.Any(z => z.IdUser == item.Id)))
                        .AnyAsync(CancellationToken);

                    var entry = body.Entries.First(x => x.IdUserEvent == item.Id);

                    var attendance = item.UserEventAttendance2s?.FirstOrDefault(x => x.IdEventIntendedForAtdCheckStudent == checkStudent.Id);

                    if (GetUserEventAttendance2.Any())
                    {
                        GetUserEventAttendance2.ForEach(x => x.IsActive = false);
                        _dbContext.Entity<TrUserEventAttendance2>().UpdateRange(GetUserEventAttendance2);
                    }

                    foreach (var ItemEventIntendedForAtdCheckStudent in IdEventIntendedForAtdCheckStudent)
                    {
                        var userEventAttendance = new TrUserEventAttendance2
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdUserEvent = entry.IdUserEvent,
                            IdEventIntendedForAtdCheckStudent = ItemEventIntendedForAtdCheckStudent,
                            IdAttendanceMappingAttendance = entry.IdAttendanceMapAttendance,
                            LateTime = !string.IsNullOrEmpty(entry.LateInMinute) ? TimeSpan.Parse(entry.LateInMinute) : default,
                            FileEvidence = entry.File,
                            Notes = entry.Note,
                            DateEvent = checkStudent.StartDate,
                            HasBeenResolved = !hasConflict
                        };
                        _dbContext.Entity<TrUserEventAttendance2>().Add(userEventAttendance);

                        foreach (var studentWorkhabit in entry.IdWorkhabits)
                        {
                            var workhabit = new TrUserEventAttendanceWorkhabit2
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdUserEventAttendance = userEventAttendance.Id,
                                IdMappingAttendanceWorkhabit = studentWorkhabit,
                            };
                            _dbContext.Entity<TrUserEventAttendanceWorkhabit2>().Add(workhabit);
                        }
                    }

                    if (eventType == EventIntendedForAttendanceStudent.Mandatory)
                    {
                        // action for replace attendance entry
                        if (!hasConflict && checkStudent.IsPrimary)
                        {
                            List<string> scheduleLessonIds = new List<string>();

                            if (!IsRepeat)
                            {
                                scheduleLessonIds = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                .Include(x => x.GeneratedScheduleStudent)
                                .Where(x
                                    => x.GeneratedScheduleStudent.IdStudent == item.IdUser
                                    && (x.ScheduleDate.Date >= StartDate.Date && x.ScheduleDate.Date <= EndDate.Date )
                                    && ((x.StartTime <= checkStudent.StartTime && x.EndTime >= checkStudent.EndTime) || (x.StartTime >= checkStudent.StartTime && x.EndTime <= checkStudent.EndTime)))
                                .Select(x => x.Id)
                                .ToListAsync(CancellationToken);
                            }
                            else
                            {
                                scheduleLessonIds = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                .Include(x => x.GeneratedScheduleStudent)
                                .Where(x
                                    => x.GeneratedScheduleStudent.IdStudent == item.IdUser
                                    && x.ScheduleDate.Date == checkStudent.StartDate.Date
                                    && ((x.StartTime <= checkStudent.StartTime && x.EndTime >= checkStudent.EndTime) || (x.StartTime >= checkStudent.StartTime && x.EndTime <= checkStudent.EndTime)))
                                .Select(x => x.Id)
                                .ToListAsync(CancellationToken);
                            }
                            

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
                                    IdAttendanceMappingAttendance = entry.IdAttendanceMapAttendance,
                                    LateTime = !string.IsNullOrEmpty(entry.LateInMinute) ? TimeSpan.Parse(entry.LateInMinute) : default,
                                    FileEvidence = entry.File,
                                    Notes = entry.Note,
                                    Status = AttendanceEntryStatus.Submitted
                                };
                                _dbContext.Entity<TrAttendanceEntry>().Add(attendanceEntry);

                                foreach (var studentWorkhabit in entry.IdWorkhabits)
                                {
                                    var attendanceEntryWorkhabit = new TrAttendanceEntryWorkhabit
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdAttendanceEntry = attendance.Id,
                                        IdMappingAttendanceWorkhabit = studentWorkhabit,
                                    };
                                    _dbContext.Entity<TrAttendanceEntryWorkhabit>().Add(attendanceEntryWorkhabit);
                                }
                            }
                        }
                    }

                    if (eventType == EventIntendedForAttendanceStudent.Excuse)
                    {
                        var IdAcademicYear = await _dbContext.Entity<MsLevel>()
                                    .Include(e => e.AcademicYear)
                                    .Where(e => e.Id == body.IdLevel)
                                    .Select(e => e.IdAcademicYear)
                                    .FirstOrDefaultAsync(CancellationToken);

                        var IdAttendanceMappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                                    .Include(e => e.Attendance)
                                    .Include(e => e.MappingAttendance)
                                    .Where(e => e.Attendance.IdAcademicYear == IdAcademicYear && e.Attendance.Code == "EA" && e.MappingAttendance.IdLevel==body.IdLevel)
                                    .Select(e => e.Id)
                                    .FirstOrDefaultAsync(CancellationToken);
                        // action for replace attendance entry
                        if (!hasConflict && checkStudent.IsPrimary)
                        {
                            List<string> scheduleLessonIds = new List<string>();

                            if (!IsRepeat)
                            {
                                scheduleLessonIds = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                .Include(x => x.GeneratedScheduleStudent)
                                .Where(x
                                    => x.GeneratedScheduleStudent.IdStudent == item.IdUser
                                    && (x.ScheduleDate.Date >= StartDate.Date && x.ScheduleDate.Date <= EndDate.Date)
                                    && ((x.StartTime <= checkStudent.StartTime && x.EndTime >= checkStudent.EndTime) || (x.StartTime >= checkStudent.StartTime && x.EndTime <= checkStudent.EndTime)))
                                .Select(x => x.Id)
                                .ToListAsync(CancellationToken);
                            }
                            else
                            {
                                scheduleLessonIds = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                                .Include(x => x.GeneratedScheduleStudent)
                                .Where(x
                                    => x.GeneratedScheduleStudent.IdStudent == item.IdUser
                                    && x.ScheduleDate.Date == checkStudent.StartDate.Date
                                    && ((x.StartTime <= checkStudent.StartTime && x.EndTime >= checkStudent.EndTime) || (x.StartTime >= checkStudent.StartTime && x.EndTime <= checkStudent.EndTime)))
                                .Select(x => x.Id)
                                .ToListAsync(CancellationToken);
                            }


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
                                    IdAttendanceMappingAttendance = IdAttendanceMappingAttendance,
                                    LateTime = !string.IsNullOrEmpty(entry.LateInMinute) ? TimeSpan.Parse(entry.LateInMinute) : default,
                                    FileEvidence = entry.File,
                                    Notes = entry.Note,
                                    Status = AttendanceEntryStatus.Submitted
                                };
                                _dbContext.Entity<TrAttendanceEntry>().Add(attendanceEntry);

                                foreach (var studentWorkhabit in entry.IdWorkhabits)
                                {
                                    var attendanceEntryWorkhabit = new TrAttendanceEntryWorkhabit
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdAttendanceEntry = attendance.Id,
                                        IdMappingAttendanceWorkhabit = studentWorkhabit,
                                    };
                                    _dbContext.Entity<TrAttendanceEntryWorkhabit>().Add(attendanceEntryWorkhabit);
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
