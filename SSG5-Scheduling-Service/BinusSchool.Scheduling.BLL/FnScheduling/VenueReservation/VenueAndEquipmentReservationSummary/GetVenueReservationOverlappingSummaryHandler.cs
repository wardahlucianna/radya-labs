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
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class GetVenueReservationOverlappingSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly IMachineDateTime _date;
        private static readonly string[] _columns = new[]
        {
            "ScheduleDate",
            "TimeBooked",
            "Building",
            "Venue",
            "ReservedBy",
        };

        public GetVenueReservationOverlappingSummaryHandler(ISchedulingDbContext context, IMachineDateTime date)
        {
            _context = context;
            _date = date;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetVenueReservationOverlappingSummaryRequest>(
                nameof(GetVenueReservationOverlappingSummaryRequest.BookingStartDate),
                nameof(GetVenueReservationOverlappingSummaryRequest.BookingEndDate));

            var response = await GetVenueReservationOverlappingSummary(request);

            var count = request.CanCountWithoutFetchDb(response.Count)
                ? response.Count
                : response.Select(a => a.IdBooking).Count();

            response = response
                .SetPagination(request)
                .ToList();

            return Request.CreateApiResult2(response as object, request.CreatePaginationProperty(count));
        }

        public async Task<List<GetVenueReservationOverlappingSummaryResponse>> GetVenueReservationOverlappingSummary(GetVenueReservationOverlappingSummaryRequest request)
        {
            var response = new List<GetVenueReservationOverlappingSummaryResponse>();

            var date = _date.ServerTime;

            var idLoggedUser = AuthInfo.UserId;

            // for testing
            // var idLoggedUser = "";

            var idSchool = await _context.Entity<MsUserSchool>()
                .Where(a => a.IdUser == idLoggedUser)
                .FirstOrDefaultAsync(CancellationToken);

            List<int> visibleVenueStatus = new List<VenueApprovalStatus>
            {
                VenueApprovalStatus.Approved,
                VenueApprovalStatus.WaitingForApproval,
                VenueApprovalStatus.NoNeedApproval,
            }.Cast<int>().ToList();

            #region schedule data
            var getScheduleLesson = _context.Entity<MsScheduleLesson>()
                .Include(a => a.Lesson)
                    .ThenInclude(lesson => lesson.LessonTeachers)
                    .ThenInclude(lessonTeacher => lessonTeacher.Staff)
                .Include(a => a.Venue.VenueMappings)
                .Where(a => request.BookingStartDate <= a.ScheduleDate
                    && request.BookingEndDate >= a.ScheduleDate)
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToList();

            var getScheduleRealization = _context.Entity<TrScheduleRealization2>()
                .Include(a => a.Venue.VenueMappings)
                .Include(a => a.Lesson.Subject)
                .Where(a => request.BookingStartDate <= a.ScheduleDate
                    && request.BookingEndDate >= a.ScheduleDate)
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
                .Where(a => request.BookingStartDate <= a.ScheduleDate
                    && request.BookingEndDate >= a.ScheduleDate
                    && !scheduleLessonIds.Contains(a.Id)
                    && (string.IsNullOrEmpty(request.IdVenue) ? true : a.IdVenue == request.IdVenue))
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToList();
            #endregion

            var venueReservations = await _context.Entity<TrVenueReservation>()
                .Include(a => a.VenueMapping.Venue)
                .Include(a => a.VenueMapping.Floor.Building)
                .Include(a => a.User)
                .Where(a => request.BookingStartDate <= a.ScheduleDate
                    && request.BookingEndDate >= a.ScheduleDate
                    && (string.IsNullOrEmpty(request.IdBuilding) ? true : a.VenueMapping.Floor.IdBuilding == request.IdBuilding)
                    && (string.IsNullOrEmpty(request.IdVenue) ? true : a.VenueMapping.IdVenue == request.IdVenue)
                    && visibleVenueStatus.Contains(a.Status)
                    && a.VenueMapping.AcademicYear.IdSchool == idSchool.IdSchool
                    && a.IsOverlapping == true)
            .ToListAsync(CancellationToken);

            if (!venueReservations.Any())
                return response;

            foreach (var venue in venueReservations)
            {
                var overlap = new List<GetVenueReservationOverlappingSummaryResponse_Overlap>();

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
                    .Select(a => new GetVenueReservationOverlappingSummaryResponse_Overlap
                    {
                        Teacher = new ItemValueVm
                        {
                            Id = a.Lesson.LessonTeachers.Select(b => b.Staff.IdBinusian).FirstOrDefault(),
                            Description = NameUtil.GenerateFullName(a.Lesson.LessonTeachers.Select(b => b.Staff.FirstName).FirstOrDefault(), a.Lesson.LessonTeachers.Select(b => b.Staff.LastName).FirstOrDefault()),
                        },
                        Time = new GetVenueReservationOverlappingSummaryResponse_Overlap_Time
                        {
                            Start = a.StartTime,
                            End = a.EndTime,
                        },
                        Subject = new ItemValueVm
                        {
                            Id = a.IdSubject,
                            Description = a.SubjectName
                        },
                        OverlapFrom = "Generate Schedule"
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
                    .Select(a => new GetVenueReservationOverlappingSummaryResponse_Overlap
                    {
                        Teacher = new ItemValueVm
                        {
                            Id = a.ScheduleRealization.IdBinusianSubtitute,
                            Description = a.ScheduleRealization.TeacherNameSubtitute,
                        },
                        Time = new GetVenueReservationOverlappingSummaryResponse_Overlap_Time
                        {
                            Start = a.ScheduleRealization.StartTime,
                            End = a.ScheduleRealization.EndTime,
                        },
                        Subject = new ItemValueVm
                        {
                            Id = a.ScheduleRealization.Lesson.IdSubject,
                            Description = a.ScheduleRealization.Lesson.Subject.Description,
                        },
                        OverlapFrom = "Generate Realization"
                    })
                    .ToList();

                overlap.AddRange(overlapRealization);

                if (checkWithScheduleLesson.Any() || checkWithScheduleRealization.Any())
                {
                    var insertOverlap = new GetVenueReservationOverlappingSummaryResponse
                    {
                        IdBooking = venue.Id,
                        ScheduleDate = venue.ScheduleDate,
                        Time = new GetVenueReservationOverlappingSummaryResponse_Time
                        {
                            Start = venue.StartTime,
                            End = venue.EndTime,
                        },
                        Building = new ItemValueVm
                        {
                            Id = venue.VenueMapping.Floor.IdBuilding,
                            Description = venue.VenueMapping.Floor.Building.Description,
                        },
                        Venue = new ItemValueVm
                        {
                            Id = venue.VenueMapping.IdVenue,
                            Description = venue.VenueMapping.Venue.Description,
                        },
                        Requester = new ItemValueVm
                        {
                            Id = venue.IdUser,
                            Description = NameUtil.GenerateFullName(venue.User.DisplayName.Trim())
                        },
                        Event = venue.EventDescription,
                        Overlap = overlap
                            .OrderBy(a => a.Time.Start)
                            .ToList()
                    };

                    response.Add(insertOverlap);
                }
            }

            if (!string.IsNullOrEmpty(request.Search))
                response = response
                    .Where(a => EF.Functions.Like(a.ScheduleDate.Date.ToString(), request.SearchPattern())
                        || EF.Functions.Like(a.Time.Start.ToString(), request.SearchPattern())
                        || EF.Functions.Like(a.Time.End.ToString(), request.SearchPattern())
                        || EF.Functions.Like(a.Building.Description, request.SearchPattern())
                        || EF.Functions.Like(a.Venue.Description, request.SearchPattern())
                        || EF.Functions.Like(a.Requester.Description, request.SearchPattern()))
                    .ToList();

            response = request.OrderBy switch
            {
                "ScheduleDate" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.ScheduleDate).ToList()
                    : response.OrderByDescending(a => a.ScheduleDate).ToList(),
                "TimeBooked" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Time.Start).ToList()
                    : response.OrderByDescending(a => a.Time.Start).ToList(),
                "Building" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Building.Description).ToList()
                    : response.OrderByDescending(a => a.Building.Description).ToList(),
                "Venue" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Venue.Description).ToList()
                    : response.OrderByDescending(a => a.Venue.Description).ToList(),
                "ReservedBy" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Requester.Description).ToList()
                    : response.OrderByDescending(a => a.Requester.Description).ToList(),
                _ => response.OrderBy(a => a.ScheduleDate).ToList()
            };

            return response;
        }
    }
}
