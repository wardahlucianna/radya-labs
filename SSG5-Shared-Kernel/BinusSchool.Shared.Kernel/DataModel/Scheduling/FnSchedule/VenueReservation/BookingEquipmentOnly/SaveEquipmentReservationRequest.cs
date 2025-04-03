using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class SaveEquipmentReservationRequest
    {
        public List<SaveEquipmentReservationRequest_Mapping> EquipmentReservationMapping { get; set; }
        public string IdSchool { get; set; }
        public string IdUserLogin { get; set; }
    }

    public class SaveEquipmentReservationRequest_Mapping
    {
        public string? IdMappingEquipmentReservation { get; set; }
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
        public string IdUser { get; set; }
        public string? IdVenue { get; set; }
        public string EventDescription { get; set; }
        public string? IdVenueReservation { get; set; }
        public string? VenueNameinEquipment { get; set; }
        public string? Notes { get; set; }
        public List<SaveEquipmentReservationRequest_Equipment> ListEquipment { get; set; }
    }

    public class SaveEquipmentReservationRequest_Equipment
    {
        public string IdEquipment { get; set; }
        public int EquipmentBorrowingQty { get; set; }
    }
}
