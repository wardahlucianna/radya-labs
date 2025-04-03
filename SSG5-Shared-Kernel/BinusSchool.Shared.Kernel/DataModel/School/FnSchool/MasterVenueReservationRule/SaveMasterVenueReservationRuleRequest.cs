using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.MasterVenueReservationRule
{
    public class SaveMasterVenueReservationRuleRequest
    {
        public int? MaxDayBookingVenue { get; set; }
        public string? MaxTimeBookingVenue { get; set; }
        public int? MaxDayDurationBookingVenue { get; set; }
        public string? VenueNotes { get; set; }
        public string StartTimeOperational { get; set; }
        public string EndTimeOperational { get; set; }
        public bool CanBookingAnotherUser { get; set; }
        public string IdSchool { get; set; }
    }
}
