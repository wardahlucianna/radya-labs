using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Helper;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class GetVenueReservationScheduleTimelineHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly IMachineDateTime _dateTime;
        private readonly GetVenueReservationUserLoginSpecialRoleHandler _getVenueReservationUserLoginSpecialRoleHandler;

        public GetVenueReservationScheduleTimelineHandler(ISchedulingDbContext context, IMachineDateTime dateTime, GetVenueReservationUserLoginSpecialRoleHandler getVenueReservationUserLoginSpecialRoleHandler)
        {
            _context = context;
            _dateTime = dateTime;
            _getVenueReservationUserLoginSpecialRoleHandler = getVenueReservationUserLoginSpecialRoleHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetVenueReservationScheduleTimelineRequest>
                (nameof(GetVenueReservationScheduleTimelineRequest.Date),
                 nameof(GetVenueReservationScheduleTimelineRequest.IdFloor));

            var response = new GetVenueReservationScheduleTimelineResponse();
            var venues = new List<GetVenueReservationScheduleTimelineResponse_Venue>();
            var bookings = new List<GetVenueReservationScheduleTimelineResponse_Booking>();
            List<int> visibleVenueStatuses = new List<VenueApprovalStatus>
            {
                VenueApprovalStatus.Approved,
                VenueApprovalStatus.WaitingForApproval,
                VenueApprovalStatus.NoNeedApproval
            }.Cast<int>().ToList();

            DateTime today = _dateTime.ServerTime;
            var idLoggedUser = AuthInfo.UserId;

            var checkUserSpecialRole = await _getVenueReservationUserLoginSpecialRoleHandler.GetVenueReservationUserLoginSpecialRole(new GetVenueReservationUserLoginSpecialRoleRequest
            {
                IdUser = idLoggedUser
            });

            var idSchool = await _context.Entity<MsUserSchool>()
                .Where(a => a.IdUser == idLoggedUser)
                .FirstOrDefaultAsync(CancellationToken);

            #region Get Academic Year
            // check period
            var checkActivePeriod = await _context.Entity<MsPeriod>()
               .Include(x => x.Grade)
                   .ThenInclude(x => x.Level)
                       .ThenInclude(x => x.AcademicYear)
               .Where(x => x.Grade.Level.AcademicYear.IdSchool == idSchool.IdSchool)
               .Where(x => request.Date.Date >= x.StartDate.Date)
               .Where(x => request.Date.Date <= x.EndDate.Date)
               .Select(x => x.Grade.Level.AcademicYear.Id).FirstOrDefaultAsync();

            var getLatestAcademicYear = await _context.Entity<MsPeriod>()
               .Include(x => x.Grade)
                   .ThenInclude(x => x.Level)
                       .ThenInclude(x => x.AcademicYear)
               .Where(x => x.Grade.Level.AcademicYear.IdSchool == idSchool.IdSchool)
               .Where(x => request.Date.Date < x.EndDate.Date)
               .Select(x => x.Grade.Level.AcademicYear.Id).FirstOrDefaultAsync();
            #endregion

            var getVenueReservationRule = await _context.Entity<MsVenueReservationRule>()
                .Where(a => a.IdSchool == idSchool.IdSchool)
                .FirstOrDefaultAsync(CancellationToken);

            var getVenueReservation = await _context.Entity<TrVenueReservation>()
                .Include(a => a.VenueMapping.Venue)
                .Include(a => a.User)
                .Where(a => a.VenueMapping.AcademicYear.IdSchool == idSchool.IdSchool
                    && a.ScheduleDate == request.Date
                    && a.VenueMapping.IdFloor == request.IdFloor
                    && (checkActivePeriod.Count() != 0 ? a.VenueMapping.IdAcademicYear == checkActivePeriod : a.VenueMapping.IdAcademicYear == getLatestAcademicYear)
                    && (visibleVenueStatuses.Contains(a.Status)))
                .ToListAsync(CancellationToken);

            var getVenueMapping = await _context.Entity<MsVenueMapping>()
                .Include(a => a.Venue)
                .Where(a => a.IdFloor == request.IdFloor
                    && (checkActivePeriod.Count() != 0 ? a.IdAcademicYear == checkActivePeriod : a.IdAcademicYear == getLatestAcademicYear))
                .OrderBy(a => a.Venue.Description.Length)
                    .ThenBy(a => a.Venue.Description)
                .ToListAsync(CancellationToken);

            var getVenueRestrict = _context.Entity<MsRestrictionBookingVenue>()
                .Where(a => request.Date.Date >= a.StartRestrictionDate.Date
                    && request.Date.Date <= a.EndRestrictionDate.Date)
                .ToList();

            // insert venues
            var insertVenues = getVenueMapping
                .Select(a => new GetVenueReservationScheduleTimelineResponse_Venue
                {
                    IdVenue = a.Venue.Id,
                    VenueDesc = a.Venue.Description,
                    Status = a.IsVenueActive == true ? 1 : 0,
                    RestrictDate = getVenueRestrict
                        .Where(b => (b.IdBuilding == a.Venue.IdBuilding && string.IsNullOrEmpty(b.IdVenue))
                            || (b.IdVenue == a.IdVenue && b.IdBuilding == a.Venue.IdBuilding))
                        .Select(b => new GetVenueReservationScheduleTimelineResponse_Venue_RestricTime {
                                            RestrictStartDate = b.StartRestrictionDate.Date == request.Date ?
                                            b.StartRestrictionDate :
                                            new DateTime(request.Date.Year, request.Date.Month, request.Date.Day, 0, 0, 0),
                                            RestrictEndDate = b.EndRestrictionDate.Date == request.Date ?
                                            b.EndRestrictionDate :
                                            new DateTime(request.Date.Year, request.Date.Month, request.Date.Day, 23, 59, 59)
                                        })
                        .ToList()
                })
                .OrderBy(a => a.VenueDesc.Length)
                    .ThenBy(a => a.VenueDesc)
                .ToList();

            venues.AddRange(insertVenues);
            response.Venues = venues;

            #region Schedule Realization
            var venueIds = venues.Select(vm => vm.IdVenue).ToList();

            var getScheduleLesson = await _context.Entity<MsScheduleLesson>()
                .Include(a => a.Lesson)
                    .ThenInclude(lesson => lesson.LessonTeachers)
                    .ThenInclude(lessonTeacher => lessonTeacher.Staff)
                .Include(a => a.Venue.VenueMappings)
                .Where(a => a.ScheduleDate.Date == request.Date)
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToListAsync(CancellationToken);

            var getScheduleRealization = await _context.Entity<TrScheduleRealization2>()
                .Include(a => a.Venue.VenueMappings)
                .Where(a => a.ScheduleDate.Date == request.Date)
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToListAsync(CancellationToken);

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

            var getScheduleLeesonRequestFloor = await _context.Entity<MsScheduleLesson>()
                .Include(a => a.Lesson)
                    .ThenInclude(lesson => lesson.LessonTeachers)
                    .ThenInclude(lessonTeacher => lessonTeacher.Staff)
                .Include(a => a.Venue.VenueMappings)
                .Where(a => a.ScheduleDate.Date == request.Date
                    && venueIds.Contains(a.IdVenue)
                    && !scheduleLessonIds.Contains(a.Id))
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToListAsync(CancellationToken);

            if (joinSchedule.Any())
            {
                var insertScheduleRealization = joinSchedule
                .Where(a => venueIds.Contains(a.ScheduleRealization.IdVenueChange)
                    && a.ScheduleRealization.IsCancel == false)
                .Select(a => new GetVenueReservationScheduleTimelineResponse_Booking
                {
                    IdVenueReservation = null,
                    ScheduleDate = a.ScheduleRealization.ScheduleDate,
                    StartTime = a.ScheduleRealization.StartTime,
                    EndTime = a.ScheduleRealization.EndTime,
                    Requester = new ItemValueVm
                    {
                        Id = a.ScheduleRealization.IdBinusianSubtitute,
                        Description = a.ScheduleRealization.TeacherNameSubtitute
                    },
                    EventName = a.ScheduleLesson.SubjectName,
                    Venue = new ItemValueVm
                    {
                        Id = a.ScheduleRealization.IdVenueChange,
                        Description = a.ScheduleRealization.VenueNameChange
                    },
                    LegendVenueStatus = -1,
                    CanEdit = false
                });

                bookings.AddRange(insertScheduleRealization);
            }

            var insertScheduleLesson = getScheduleLeesonRequestFloor
                .Select(a => new GetVenueReservationScheduleTimelineResponse_Booking
                {
                    IdVenueReservation = null,
                    ScheduleDate = a.ScheduleDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Requester = new ItemValueVm
                    {
                        Id = a.Lesson.LessonTeachers.Select(b => b.IdUser).FirstOrDefault(),
                        Description = NameUtil.GenerateFullName(a.Lesson.LessonTeachers.Select(b => b.Staff.FirstName).FirstOrDefault(), a.Lesson.LessonTeachers.Select(b => b.Staff.LastName).FirstOrDefault())
                    },
                    EventName = a.SubjectName,
                    Venue = new ItemValueVm
                    {
                        Id = a.IdVenue,
                        Description = a.VenueName
                    },
                    LegendVenueStatus = -1,
                    CanEdit = false
                });

            bookings.AddRange(insertScheduleLesson);

            response.Bookings = bookings;
            #endregion

            if (checkUserSpecialRole == null || (checkUserSpecialRole != null && checkUserSpecialRole.CanOverrideAnotherReservation == false && checkUserSpecialRole.AllSuperAccess == false))
            {
                foreach (var item in getVenueReservation)
                {
                    var insertBooking = new GetVenueReservationScheduleTimelineResponse_Booking
                    {
                        IdVenueReservation = item.Id,
                        ScheduleDate = item.ScheduleDate,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        PreparationTime = item.PreparationTime ?? null,
                        CleanUpTime = item.CleanUpTime ?? null,
                        Requester = new ItemValueVm
                        {
                            Id = item.IdUser,
                            Description = item.User.DisplayName
                        },
                        EventName = item.EventDescription,
                        Venue = new ItemValueVm
                        {
                            Id = item.VenueMapping.IdVenue,
                            Description = item.VenueMapping.Venue.Description
                        },
                        LegendVenueStatus = item.Status,
                        CanEdit = BookingVenueAndEquipmentValidationHelper.ValidateEdit(new BookingVenueAndEquipmentValidationParams
                        {
                            Today = today,
                            ScheduleDate = item.ScheduleDate,
                            StartTime = item.StartTime,
                            MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                            MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                            AllSuperAccess = false,
                            CanOverride = false,
                            IdLoggedUser = idLoggedUser,
                            CreatedBy = item.UserIn,
                            CreatedFor = item.IdUser,
                            ApprovalStatus = item.Status,
                            IsOverlapping = item.IsOverlapping
                        })
                    };

                    bookings.Add(insertBooking);
                }

                response.Bookings = bookings;
            }
            else
            {
                var insertBooking = getVenueReservation
                    .Select(a => new GetVenueReservationScheduleTimelineResponse_Booking
                    {
                        IdVenueReservation = a.Id,
                        ScheduleDate = a.ScheduleDate,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        PreparationTime = a.PreparationTime ?? null,
                        CleanUpTime = a.CleanUpTime ?? null,
                        Requester = new ItemValueVm
                        {
                            Id = a.IdUser,
                            Description = a.User.DisplayName
                        },
                        EventName = a.EventDescription,
                        Venue = new ItemValueVm
                        {
                            Id = a.VenueMapping.IdVenue,
                            Description = a.VenueMapping.Venue.Description
                        },
                        LegendVenueStatus = a.Status,
                        CanEdit = BookingVenueAndEquipmentValidationHelper.ValidateEdit(new BookingVenueAndEquipmentValidationParams
                        {
                            Today = today,
                            ScheduleDate = a.ScheduleDate,
                            StartTime = a.StartTime,
                            MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                            MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                            AllSuperAccess = checkUserSpecialRole.AllSuperAccess,
                            CanOverride = checkUserSpecialRole.CanOverrideAnotherReservation,
                            IdLoggedUser = idLoggedUser,
                            CreatedBy = a.UserIn,
                            CreatedFor = a.IdUser,
                            ApprovalStatus = a.Status,
                            IsOverlapping = a.IsOverlapping
                        })
                    });

                bookings.AddRange(insertBooking);

                response.Bookings = bookings; 
            }

            return Request.CreateApiResult2(response as object);
        }
    }
}
