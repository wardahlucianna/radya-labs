using System;

namespace BinusSchool.Data.Model.School.FnSchool.MasterVenueReservationRule
{
    public class GetMasterVenueReservationRuleResult
    {
        public string IdVenueReservationRule { get; set; }
        public int? MaxDayBookingVenue { get; set; }
        public TimeSpan? MaxTimeBookingVenue { get; set; }
        public int? MaxDayDurationBookingVenue { get; set; }
        public string? VenueNotes { get; set; }
        public TimeSpan StartTimeOperational { get; set; }
        public TimeSpan EndTimeOperational { get; set; }
        public bool CanBookingAnotherUser { get; set; }
        public string LastUserUpdated { get; set; }
        public DateTime? LastDateUpdated { get; set; }

    }
}
