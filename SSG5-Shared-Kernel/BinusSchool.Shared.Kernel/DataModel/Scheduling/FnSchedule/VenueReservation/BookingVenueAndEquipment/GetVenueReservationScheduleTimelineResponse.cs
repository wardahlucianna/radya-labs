using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class GetVenueReservationScheduleTimelineResponse
    {
        public List<GetVenueReservationScheduleTimelineResponse_Venue> Venues { get; set; }
        public List<GetVenueReservationScheduleTimelineResponse_Booking> Bookings { get; set; }
    }

    public class GetVenueReservationScheduleTimelineResponse_Venue
    {
        public string IdVenue { get; set; }
        public string VenueDesc { get; set; }
        public int Status { get; set; }
        public List<GetVenueReservationScheduleTimelineResponse_Venue_RestricTime> RestrictDate { get; set; }
    }

    public class GetVenueReservationScheduleTimelineResponse_Booking
    {
        public string IdVenueReservation { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? PreparationTime { get; set; }
        public int? CleanUpTime { get; set; }
        public ItemValueVm Requester { get; set; }
        public string EventName { get; set; }
        public ItemValueVm Venue { get; set; }
        public int LegendVenueStatus { get; set; }
        public bool CanEdit { get; set; }
    }
    
    public class GetVenueReservationScheduleTimelineResponse_Venue_RestricTime
    {
        public DateTime RestrictStartDate { get; set; }
        public DateTime RestrictEndDate { get; set; }
    }
}
