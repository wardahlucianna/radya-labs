using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class GetVenueReservationOverlapCheckRequest
    {
        public string IdVenueReservation { get; set; }
        public string IdVenue { get; set; }
        public string Requester { get; set; }
        public string EventDescription { get; set; }
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
        public List<string> Recurrence { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? PreparationTime { get; set; }
        public int? CleanUpTime { get; set; }
        public string Note { get; set; }
        public GetVenueReservationOverlapCheckRequest_File FileUpload { get; set; }
        public List<GetVenueReservationOverlapCheckRequest_Equipment> Equipments { get; set; }
    }

    public class GetVenueReservationOverlapCheckRequest_File
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
        public string Url { get; set; }
    }

    public class GetVenueReservationOverlapCheckRequest_Equipment
    {
        public string IdEquipment { get; set; }
        public int EquipmentBorrowingQty { get; set; }
    }
}
