using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class UpdateVenueReservationPrepAndCleanTimeRequest
    {
        public string IdUser { get; set; }
        public string IdVenueReservation { get; set; }
        public int? PreparationTime { get; set; }
        public int? CleanUpTime { get; set; }
    }
}
