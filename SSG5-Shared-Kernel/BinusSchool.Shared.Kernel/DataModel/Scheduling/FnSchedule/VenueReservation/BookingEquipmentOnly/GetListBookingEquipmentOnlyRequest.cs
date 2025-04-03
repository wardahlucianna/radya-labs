using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class GetListBookingEquipmentOnlyRequest
    {
        public string IdSchool { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool GetAllData { get; set; } = false;
    }
}
