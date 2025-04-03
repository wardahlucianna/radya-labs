using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail
{
    public class SendEmailBulkVenueAndEquipmentReservationForUserRequest
    {
        public List<string> IdBooking { get; set; }
        public string Action { get; set; }
        public List<DateTime> Recurrence { get; set; }
        public VenueApprovalStatus? ApprovalStatus { get; set; }
        public List<NotAvailableEquipmentForUser> NotAvailableEquipments { get; set; }
        public List<CanceledEquipmentForUser> CanceledEquipments { get; set; }
    }
    public class NotAvailableEquipmentForUser
    {
        public string ScheduleDate { get; set; }
        public string EquipmentName { get; set; }
        public string BorrowingQty { get; set; }
        public string StockQty { get; set; }
    }

    public class CanceledEquipmentForUser
    {
        public string EquipmentName { get; set; }
        public int EquipmentQty { get; set; }
    }
}
