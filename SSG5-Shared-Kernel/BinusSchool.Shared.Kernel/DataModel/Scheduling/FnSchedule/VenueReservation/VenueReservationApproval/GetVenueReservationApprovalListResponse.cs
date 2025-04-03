using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval
{
    public class GetVenueReservationApprovalListResponse
    {
        public string IdBooking { get; set; }
        public DateTime ScheduleDate { get; set; }
        public ItemValueVm Venue { get; set; }
        public GetVenueReservationApprovalListResponse_Time Time { get; set; }
        public string EventName { get; set; }
        public ItemValueVm Requester { get; set; }
        public GetVenueReservationApprovalListResponse_BookingStatus BookingStatus { get; set; }
        public GetVenueReservationApprovalListResponse_Action Action { get; set; }
    }

    public class GetVenueReservationApprovalListResponse_Time
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }

    public class GetVenueReservationApprovalListResponse_BookingStatus
    {
        public int IdBooking { get; set; }
        public string BookingDesc { get; set; }
        public string RejectionReason { get; set; }
        public bool IsOverlapping { get; set; }
    }

    public class GetVenueReservationApprovalListResponse_Action
    {
        public bool CanApproveReject { get; set; }
    }
}
