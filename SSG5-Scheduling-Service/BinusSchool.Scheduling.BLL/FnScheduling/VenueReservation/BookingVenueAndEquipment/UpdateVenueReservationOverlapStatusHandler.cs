using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class UpdateVenueReservationOverlapStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly IMachineDateTime _dateTime;
        private IDbContextTransaction _transaction;
        private readonly GetVenueReservationUserLoginSpecialRoleHandler _specialRoleHandler;
        private readonly SendEmailBulkOverlapStatus _sendEmail;

        public UpdateVenueReservationOverlapStatusHandler(ISchedulingDbContext context, IMachineDateTime dateTime, GetVenueReservationUserLoginSpecialRoleHandler specialRoleHandler, SendEmailBulkOverlapStatus sendEmail)
        {
            _context = context;
            _dateTime = dateTime;
            _specialRoleHandler = specialRoleHandler;
            _sendEmail = sendEmail;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<UpdateVenueReservationOverlapStatusRequest, UpdateVenueReservationOverlapStatusValidator>();

            DateTime date = _dateTime.ServerTime;

            var checkUserSpecialRole = await _specialRoleHandler.GetVenueReservationUserLoginSpecialRole(new GetVenueReservationUserLoginSpecialRoleRequest
            {
                IdUser = request.IdUser,
            });

            if (request.IdUser != "00000000-0000-0000-0000-000000000000")
            {
                if (checkUserSpecialRole == null || (checkUserSpecialRole != null && checkUserSpecialRole.CanOverrideAnotherReservation == false && checkUserSpecialRole.AllSuperAccess == false))
                    throw new BadRequestException("This user is not Admin");
            }

            List<int> visibleVenueStatuses = new List<VenueApprovalStatus>
            {
                VenueApprovalStatus.Approved,
                VenueApprovalStatus.WaitingForApproval,
                VenueApprovalStatus.NoNeedApproval,
            }.Cast<int>().ToList();

            var overlapList = new List<SendEmailBulkOverlapStatusRequest_Overlap>();

            #region get schedule
            var getScheduleLesson = _context.Entity<MsScheduleLesson>()
                .Include(a => a.Lesson)
                    .ThenInclude(lesson => lesson.LessonTeachers)
                    .ThenInclude(lessonTeacher => lessonTeacher.Staff)
                .Include(a => a.Venue.VenueMappings)
                .Where(a => date.Date <= a.ScheduleDate
                    && date.Date.AddDays(7) >= a.ScheduleDate
                    && a.AcademicYear.IdSchool == request.IdSchool)
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToList();

            var getScheduleRealization = _context.Entity<TrScheduleRealization2>()
                .Include(a => a.Venue.VenueMappings)
                .Include(a => a.Lesson.Subject)
                .Where(a => date.Date <= a.ScheduleDate
                    && date.Date.AddDays(7) >= a.ScheduleDate
                    && a.AcademicYear.IdSchool == request.IdSchool)
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToList();

            var joinSchedule = from scheduleLesson in getScheduleLesson
                               join scheduleRealization in getScheduleRealization
                               on new { scheduleLesson.ScheduleDate, scheduleLesson.IdLesson, scheduleLesson.SessionID }
                               equals new { scheduleRealization.ScheduleDate, scheduleRealization.IdLesson, scheduleRealization.SessionID }
                               select new
                               {
                                   ScheduleLesson = scheduleLesson,
                                   ScheduleRealization = scheduleRealization
                               };

            var scheduleLessonIds = new List<string>();

            if (joinSchedule.Any())
                scheduleLessonIds = joinSchedule.Select(sl => sl.ScheduleLesson.Id).ToList();

            var getScheduleLeesonRequestFloor = _context.Entity<MsScheduleLesson>()
                .Include(a => a.Lesson)
                    .ThenInclude(lesson => lesson.LessonTeachers)
                    .ThenInclude(lessonTeacher => lessonTeacher.Staff)
                .Include(a => a.Venue.VenueMappings)
                .Where(a => date.Date <= a.ScheduleDate
                    && date.Date.AddDays(7) >= a.ScheduleDate
                    && a.AcademicYear.IdSchool == request.IdSchool
                    && !scheduleLessonIds.Contains(a.Id))
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToList();
            #endregion

            var getVenueReservation = await _context.Entity<TrVenueReservation>()
                .Include(a => a.VenueMapping.Venue)
                .Include(a => a.VenueMapping.Floor.Building)
                .Include(a => a.User)
                .Where(a => date.Date <= a.ScheduleDate.Date
                    && date.Date.AddDays(7) >= a.ScheduleDate.Date
                    && visibleVenueStatuses.Contains(a.Status)
                    && a.IsOverlapping != true
                    && a.VenueMapping.AcademicYear.IdSchool == request.IdSchool)
                .ToListAsync (CancellationToken);

            try
            {
                _transaction = await _context.BeginTransactionAsync(CancellationToken);

                foreach (var venue in getVenueReservation)
                {
                    var overlap = new List<SendEmailBulkOverlapStatusRequest_Overlap_Overlap>();

                    TimeSpan StartTime = venue.StartTime.Subtract(TimeSpan.FromMinutes(venue.PreparationTime ?? 0));
                    TimeSpan EndTime = venue.EndTime.Add(TimeSpan.FromMinutes(venue.CleanUpTime ?? 0));

                    // check with schedule lesson

                    var checkWithScheduleLesson = getScheduleLeesonRequestFloor
                        .Distinct()
                        .Where(a => venue.ScheduleDate == a.ScheduleDate
                            && StartTime < a.EndTime
                            && EndTime > a.StartTime
                            && a.IdVenue == venue.VenueMapping.IdVenue)
                        .ToList();

                    var overlapLesson = checkWithScheduleLesson
                        .Select(a => new SendEmailBulkOverlapStatusRequest_Overlap_Overlap
                        {
                            Teacher = NameUtil.GenerateFullName(a.Lesson.LessonTeachers.Select(b => b.Staff.FirstName).FirstOrDefault(), a.Lesson.LessonTeachers.Select(b => b.Staff.LastName).FirstOrDefault()),
                            Building = venue.VenueMapping.Floor.Building.Description,
                            Venue = venue.VenueMapping.Venue.Description,
                            Date = a.ScheduleDate.Date,
                            Start = a.StartTime,
                            End = a.EndTime,
                            Subject = a.SubjectName
                        })
                        .ToList();

                    overlap.AddRange(overlapLesson);

                    // check with schedule realization

                    var checkWithScheduleRealization = joinSchedule
                        .Distinct()
                        .Where(a => venue.ScheduleDate == a.ScheduleRealization.ScheduleDate
                            && StartTime < a.ScheduleRealization.EndTime
                            && EndTime > a.ScheduleRealization.StartTime
                            && a.ScheduleRealization.IdVenueChange == venue.VenueMapping.IdVenue
                            && a.ScheduleRealization.IsCancel == false)
                        .ToList();

                    var overlapRealization = checkWithScheduleRealization
                        .Select(a => new SendEmailBulkOverlapStatusRequest_Overlap_Overlap
                        {
                            Teacher = a.ScheduleRealization.TeacherNameSubtitute,
                            Building = venue.VenueMapping.Floor.Building.Description,
                            Venue = venue.VenueMapping.Venue.Description,
                            Date = a.ScheduleRealization.ScheduleDate.Date,
                            Start = a.ScheduleRealization.StartTime,
                            End = a.ScheduleRealization.EndTime,
                            Subject = a.ScheduleRealization.Lesson.Subject.Description,
                        })
                        .ToList();

                    overlap.AddRange(overlapRealization);

                    if (checkWithScheduleLesson.Any() || checkWithScheduleRealization.Any())
                    {
                        venue.IsOverlapping = true;

                        _context.Entity<TrVenueReservation>().Update(venue);

                        var insertOverlap = new SendEmailBulkOverlapStatusRequest_Overlap
                        {
                            IdBooking = venue.Id,
                            Overlaps = overlap
                                .OrderBy(a => a.Start)
                                .ToList()
                        };

                        overlapList.Add(insertOverlap);
                    }
                }

                if (overlapList.Count > 0)
                    await _sendEmail.SendEmail(new SendEmailBulkOverlapStatusRequest
                    {
                        IdSchool = request.IdSchool,
                        Overlaps = overlapList
                    });

                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();

                throw new Exception(ex.Message.ToString());
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
