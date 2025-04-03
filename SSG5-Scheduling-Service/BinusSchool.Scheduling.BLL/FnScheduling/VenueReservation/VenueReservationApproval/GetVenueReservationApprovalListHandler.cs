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
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Helper;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.Helper;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval
{
    public class GetVenueReservationApprovalListHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;

        public GetVenueReservationApprovalListHandler(ISchedulingDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetVenueReservationApprovalListRequest>
                (nameof(GetVenueReservationApprovalListRequest.IdUser),
                 nameof(GetVenueReservationApprovalListRequest.IdSchool),
                 nameof(GetVenueReservationApprovalListRequest.BookingStartDate),
                 nameof(GetVenueReservationApprovalListRequest.BookingEndDate));

            var response = new List<GetVenueReservationApprovalListResponse>();

            List<int> visibleVenueStatuses = new List<VenueApprovalStatus>
            {
                VenueApprovalStatus.Approved,
                VenueApprovalStatus.Rejected,
                VenueApprovalStatus.WaitingForApproval
            }.Cast<int>().ToList();

            var getVenueMappingApproval = await _context.Entity<MsVenueMappingApproval>()
                .Where(a => a.IdBinusian == request.IdUser)
                .ToListAsync(CancellationToken);

            var getVenueReservation = await _context.Entity<TrVenueReservation>()
                .Include(a => a.VenueMapping.Venue)
                .Include(a => a.User)
                .Where(a => a.VenueMapping.AcademicYear.IdSchool == request.IdSchool
                    && request.BookingStartDate <= a.ScheduleDate
                    && request.BookingEndDate >= a.ScheduleDate
                    && (string.IsNullOrEmpty(request.IdVenue) || a.VenueMapping.IdVenue == request.IdVenue)
                    && (request.BookingStatus == null
                        ? (visibleVenueStatuses.Contains(a.Status))
                        : (VenueApprovalStatus)a.Status == request.BookingStatus))
                .ToListAsync(CancellationToken);

            getVenueReservation = getVenueReservation
                .Where(a => getVenueMappingApproval.Any(b => b.IdVenueMapping == a.IdVenueMapping))
                .ToList();

            var getData = getVenueReservation
                    .Select(a => new GetVenueReservationApprovalListResponse
                    {
                        IdBooking = a.Id,
                        ScheduleDate = a.ScheduleDate,
                        Venue = new ItemValueVm
                        {
                            Id = a.VenueMapping.IdVenue,
                            Description = a.VenueMapping.Venue.Description
                        },
                        Time = new GetVenueReservationApprovalListResponse_Time
                        {
                            Start = a.StartTime,
                            End = a.EndTime,
                        },
                        EventName = a.EventDescription,
                        Requester = new ItemValueVm
                        {
                            Id = a.IdUser,
                            Description = a.User.DisplayName
                        },
                        BookingStatus = new GetVenueReservationApprovalListResponse_BookingStatus
                        {
                            IdBooking = a.Status,
                            BookingDesc = VenueReservationApprovalStatusHelper.ApprovalStatus(a.Status),
                            RejectionReason = a.RejectionReason,
                            IsOverlapping = a.IsOverlapping
                        },
                        Action = new GetVenueReservationApprovalListResponse_Action
                        {
                            CanApproveReject = a.Status == 3 ? true : false,
                        }
                    });

            response.AddRange(getData);

            return Request.CreateApiResult2(response as object);
        }
    }
}
