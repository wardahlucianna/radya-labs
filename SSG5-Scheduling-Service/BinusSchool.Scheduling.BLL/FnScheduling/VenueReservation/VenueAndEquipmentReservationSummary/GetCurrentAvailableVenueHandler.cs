using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Spreadsheet;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class GetCurrentAvailableVenueHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly GetVenueReservationUserLoginSpecialRoleHandler _getVenueReservationUserLoginSpecialRoleHandler;
        public GetCurrentAvailableVenueHandler(ISchedulingDbContext dbContext, GetVenueReservationUserLoginSpecialRoleHandler getVenueReservationUserLoginSpecialRoleHandler)
        {
            _dbContext = dbContext;
            _getVenueReservationUserLoginSpecialRoleHandler = getVenueReservationUserLoginSpecialRoleHandler;
        }
        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCurrentAvailableVenueRequest>(
                nameof(GetCurrentAvailableVenueRequest.BookingStartDate),
                nameof(GetCurrentAvailableVenueRequest.BookingEndDate),
                nameof(GetCurrentAvailableVenueRequest.StartTime),
                nameof(GetCurrentAvailableVenueRequest.EndTime));

            if (param.StartTime > param.EndTime)
            {
                throw new Exception("Start time cannot be later than the end time");
            }

            if (param.BookingStartDate > param.BookingEndDate)
            {
                throw new Exception("Booking start date cannot be later than the booking end date");
            }

            var idLoggedUser = AuthInfo.UserId;

            // Generate a list of time windows for each day between BookingStartDate and BookingEndDate
            // The rule here is to treat each day in the range as an independent booking day.
            // For example, if BookingStartDate is November 1, 2024, and BookingEndDate is November 5, 2024,
            // with StartTime at 08:00 and EndTime at 15:00, this means:
            // - November 1, 2024: booking window is 08:00 to 15:00
            // - November 2, 2024: booking window is 08:00 to 15:00
            // - November 3, 2024: booking window is 08:00 to 15:00
            // - ...
            // This does NOT mean a continuous booking from November 1 at 08:00 to November 5 at 15:00.
            var dailyTimeWindows = Enumerable
                .Range(0, (param.BookingEndDate - param.BookingStartDate).Days + 1)
                .Select(offset => new
                {
                    DayStart = param.BookingStartDate.Date.AddDays(offset).Add(param.StartTime),
                    DayEnd = param.BookingStartDate.Date.AddDays(offset).Add(param.EndTime)
                })
                .ToList();

            var idSchool = await _dbContext.Entity<MsUserSchool>()
                .Where(a => a.IdUser == idLoggedUser)
                .Select(x => x.IdSchool)
                .FirstOrDefaultAsync(CancellationToken);

            List<int> visibleVenueStatuses = new List<VenueApprovalStatus>
            {
                VenueApprovalStatus.Approved,
                VenueApprovalStatus.WaitingForApproval,
                VenueApprovalStatus.NoNeedApproval
            }.Cast<int>().ToList();

            var getActiveAY = await _dbContext.Entity<MsPeriod>()
                .Include(x => x.Grade)
                .Include(x => x.Grade.Level)
                .Include(x => x.Grade.Level.AcademicYear)
                .Where(x => x.StartDate.Date <= param.BookingStartDate.Date.Add(param.StartTime) && param.BookingEndDate.Date.Add(param.EndTime) <= x.EndDate.Date)
                .Where(x => x.Grade.Level.AcademicYear.IdSchool == idSchool)
                .OrderByDescending(x => x.StartDate)
                .Select(x => new
                {
                    IdAcademicYear = x.Grade.Level.AcademicYear.Id,
                    AcademicYearDescription = x.Grade.Level.AcademicYear.Description,
                })
                .FirstOrDefaultAsync(CancellationToken);

            var getVenueReservationRule = await _dbContext.Entity<MsVenueReservationRule>()
                .Where(a => a.IdSchool == idSchool &&
                            (a.StartTimeOperational >= param.StartTime ||
                            a.EndTimeOperational <= param.EndTime))
                .FirstOrDefaultAsync(CancellationToken);

            if (getVenueReservationRule != null)
            {
                throw new Exception("Start and/or end time are outside operational time");
            }

            var getVenueMapping = await _dbContext.Entity<MsVenueMapping>()
                .Include(a => a.Venue)
                    .ThenInclude(a => a.Building)
                .Include(a => a.ReservationOwner)
                .Where(x => (string.IsNullOrEmpty(param.IdBuilding) || x.Venue.IdBuilding == param.IdBuilding) &&
                            (string.IsNullOrEmpty(param.IdVenue) || x.IdVenue == param.IdVenue) &&
                            x.IdAcademicYear == getActiveAY.IdAcademicYear)
                .ToListAsync(CancellationToken);

            var getVenueReservation = await _dbContext.Entity<TrVenueReservation>()
                .Include(a => a.VenueMapping.Venue)
                .Include(a => a.User)
                .Where(a => getVenueMapping.Select(x => x.Id).Contains(a.IdVenueMapping) &&
                            visibleVenueStatuses.Contains(a.Status))
                .ToListAsync(CancellationToken);

            // The filter ensures that any equipment reservation that overlaps with these daily windows
            // is included. For example:
            // - A transaction on November 3 from 10:00 to 11:00 will be shown, as it falls within the daily window.
            // - A transaction on November 3 from 16:00 to 17:00 will NOT be shown, as it falls outside the 08:00 to 15:00 window.
            getVenueReservation = getVenueReservation
                .Where(a => dailyTimeWindows.Any(window =>
                    a.ScheduleDate.Date.Add(a.StartTime).Subtract(TimeSpan.FromMinutes(a.PreparationTime ?? 0)) < window.DayEnd &&
                    a.ScheduleDate.Date.Add(a.EndTime).Add(TimeSpan.FromMinutes(a.CleanUpTime ?? 0)) > window.DayStart)
                )
                .ToList();

            var getVenueRestrict = await _dbContext.Entity<MsRestrictionBookingVenue>()
                .Where(a => a.StartRestrictionDate <= param.BookingEndDate &&
                            param.BookingStartDate <= a.EndRestrictionDate)
                .ToListAsync(CancellationToken);

            getVenueRestrict = getVenueRestrict
                .Where(a => dailyTimeWindows.Any(window =>
                    a.StartRestrictionDate <= window.DayEnd &&
                    window.DayStart <= a.EndRestrictionDate)
                )
                .ToList();

            var getScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                .Include(a => a.Venue.VenueMappings)
                .Where(a => a.ScheduleDate.Date >= param.BookingStartDate.Date && a.ScheduleDate.Date <= param.BookingEndDate.Date &&
                            getVenueMapping.Select(x => x.IdVenue).Contains(a.IdVenue))
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToListAsync(CancellationToken);

            getScheduleLesson = getScheduleLesson
                .Where(x => dailyTimeWindows.Any(window =>
                    x.ScheduleDate.Date.Add(x.StartTime) < window.DayEnd &&
                    x.ScheduleDate.Date.Add(x.EndTime) > window.DayStart)
                )
                .ToList();

            var getScheduleRealization = await _dbContext.Entity<TrScheduleRealization2>()
                .Include(a => a.Venue.VenueMappings)
                .Where(a => a.ScheduleDate.Date >= param.BookingStartDate.Date && a.ScheduleDate.Date <= param.BookingEndDate.Date)
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToListAsync(CancellationToken);

            getScheduleRealization = getScheduleRealization
                .Where(x => dailyTimeWindows.Any(window =>
                    x.ScheduleDate.Date.Add(x.StartTime) < window.DayEnd &&
                    x.ScheduleDate.Date.Add(x.EndTime) > window.DayStart)
                )
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

            var getScheduleLeesonRequestFloor = getScheduleLesson
                .Where(x => getVenueMapping.Select(y => y.IdVenue).Contains(x.IdVenue) &&
                            !joinSchedule.Select(y => y.ScheduleLesson.Id).Contains(x.Id))
                .ToList();

            var venueMapping = getVenueMapping
                .Where(x => !getVenueRestrict.Where(y => y.IdVenue != null).Select(y => y.IdVenue).Contains(x.IdVenue) &&
                            !getVenueRestrict.Where(y => y.IdVenue == null).Select(y => y.IdBuilding).Contains(x.Venue.IdBuilding) &&
                            !getVenueReservation.Select(y => y.IdVenueMapping).Contains(x.Id) &&
                            !joinSchedule.Select(y => y.ScheduleRealization.IdVenueChange).Contains(x.IdVenue) &&
                            !getScheduleLeesonRequestFloor.Select(y => y.IdVenue).Contains(x.IdVenue) &&
                            x.IsVenueActive)
                .Select(x => new GetCurrentAvailableVenueResult
                {
                    Building = new ItemValueVm
                    {
                        Id = x.Venue.IdBuilding,
                        Description = x.Venue.Building.Description
                    },
                    Venue = new ItemValueVm
                    {
                        Id = x.IdVenue,
                        Description = x.Venue.Description
                    },
                    PicOwner = new NameValueVm
                    {
                        Id = x.IdReservationOwner,
                        Name = x.ReservationOwner?.OwnerName
                    }
                })
                .ToList();

            return Request.CreateApiResult2(venueMapping as object);
        }
    }
}
