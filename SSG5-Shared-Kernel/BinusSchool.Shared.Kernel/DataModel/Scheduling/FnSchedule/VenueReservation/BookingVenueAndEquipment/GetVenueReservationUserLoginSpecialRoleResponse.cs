using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class GetVenueReservationUserLoginSpecialRoleResponse
    {
        public int SpecialDurationBookingTotalDay { get; set; }
        public bool CanOverrideAnotherReservation { get; set; }
        public bool AllSuperAccess { get; set; }
    }
}
