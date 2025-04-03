using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class GetVenueReservationOverlappingSummaryResponse
    {
        public string IdBooking { get; set; }
        public DateTime ScheduleDate { get; set; }
        public GetVenueReservationOverlappingSummaryResponse_Time Time { get; set; }
        public ItemValueVm Building { get; set; }
        public ItemValueVm Venue { get; set; }
        public ItemValueVm Requester { get; set; }
        public string Event { get; set; }
        public List<GetVenueReservationOverlappingSummaryResponse_Overlap> Overlap { get; set; }
    }

    public class GetVenueReservationOverlappingSummaryResponse_Time
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }

    public class GetVenueReservationOverlappingSummaryResponse_Overlap
    {
        public ItemValueVm Teacher { get; set; }
        public GetVenueReservationOverlappingSummaryResponse_Overlap_Time Time { get; set; }
        public ItemValueVm Subject { get; set; }
        public string OverlapFrom { get; set; }
    }

    public class GetVenueReservationOverlappingSummaryResponse_Overlap_Time
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }
}
