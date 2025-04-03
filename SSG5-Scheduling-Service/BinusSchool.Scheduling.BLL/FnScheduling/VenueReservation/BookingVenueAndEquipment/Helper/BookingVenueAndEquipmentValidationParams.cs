using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Helper
{
    public class BookingVenueAndEquipmentValidationParams
    {
        public DateTime Today { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public int? MaxDayBooking { get; set; }
        public TimeSpan? MaxTimeBooking { get; set; }
        public string IdLoggedUser { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedFor { get; set; }
        public bool CanOverride { get; set; }
        public bool AllSuperAccess { get; set; }
        public int ApprovalStatus { get; set; }
        public bool IsOverlapping { get; set; }
        public bool CheckApprovalStatus { get; set; } = true;
    }
}
