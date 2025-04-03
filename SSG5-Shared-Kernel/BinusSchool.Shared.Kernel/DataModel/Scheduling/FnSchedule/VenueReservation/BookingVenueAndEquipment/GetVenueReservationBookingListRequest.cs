using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class GetVenueReservationBookingListRequest
    {
        public string IdSchool { get; set; }
        public string IdUser { get; set; }
        public DateTime BookingStartDate { get; set; }
        public DateTime BookingEndDate { get; set; }
        public string IdVenue { get; set; }
        public VenueApprovalStatus? BookingStatus { get; set; }
    }
}
