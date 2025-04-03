using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class SaveVenueReservationBookingRequest
    {
        public string IdVenueReservation { get; set; }
        public string IdVenue { get; set; }
        public string Requester { get; set; }
        public string EventDescription { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? PreparationTime { get; set; }
        public int? CleanUpTime { get; set; }
        public string Note { get; set; }
        public SaveVenueReservationBookingRequest_File FileUpload { get; set; }
        public List<SaveVenueReservationBookingRequest_AdditionalEquipment> AdditionalEquipments { get; set; }
    }

    public class SaveVenueReservationBookingRequest_File
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
        public string Url { get; set; }
    }

    public class SaveVenueReservationBookingRequest_AdditionalEquipment
    {
        public string IdEquipment { get; set; }
        public int EquipmentBorrowingQty { get; set; }
    }
}
