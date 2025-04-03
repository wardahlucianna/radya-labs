using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail
{
    public class SendEmailBulkVenueAndEquipmentReservationForPICRequest
    {
        public List<string> IdBooking { get; set; }
        public string Action { get; set; }
        public List<DateTime> Recurrence { get; set; }
        public List<NotAvailableEquipmentForPIC> NotAvailableEquipments { get; set; }
        public List<CanceledEquipmentForPIC> CanceledEquipments { get; set; }
    }
    public class NotAvailableEquipmentForPIC
    {
        public string ScheduleDate { get; set; }
        public string EquipmentName { get; set; }
        public string BorrowingQty { get; set; }
        public string StockQty { get; set; }
    }

    public class CanceledEquipmentForPIC
    {
        public string EquipmentName { get; set; }
        public int EquipmentQty { get; set; }
    }
}
