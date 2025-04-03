using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class DeleteBookingEquipmentOnlyRequest
    {
        public List<string> IdMappingEquipmentReservations { get; set; }
        public string IdSchool { get; set; }
    }
}
