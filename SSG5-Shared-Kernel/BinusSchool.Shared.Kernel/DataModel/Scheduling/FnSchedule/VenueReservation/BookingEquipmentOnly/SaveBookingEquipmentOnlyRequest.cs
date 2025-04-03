using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using NPOI.OpenXmlFormats.Dml;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class SaveBookingEquipmentOnlyRequest
    {
        public string IdSchool { get; set; }
        public string? IdMappingEquipmentReservation { get; set; }
        public string IdUser { get; set; }
        public string EventDescription { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? IdVenue { get; set; }
        public string? VenueNameinEquipment { get; set; }
        public string? Notes { get; set; }
        public List<ListEquipmentForBookingEquipmentOnly> ListEquipment { get; set; }
    }
}
