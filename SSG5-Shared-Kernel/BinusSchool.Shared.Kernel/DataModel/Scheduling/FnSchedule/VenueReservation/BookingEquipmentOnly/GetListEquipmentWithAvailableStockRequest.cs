using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class GetListEquipmentWithAvailableStockRequest
    {
        public string IdSchool { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? IdMappingEquipmentReservation { get; set; }
    }
}
