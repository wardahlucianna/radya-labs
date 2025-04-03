using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail
{
    public class SendEmailBulkOverlapStatusRequest
    {
        public string IdSchool { get; set; }
        public List<SendEmailBulkOverlapStatusRequest_Overlap> Overlaps { get; set; }
    }

    public class SendEmailBulkOverlapStatusRequest_Overlap
    {
        public string IdBooking { get; set; }
        public List<SendEmailBulkOverlapStatusRequest_Overlap_Overlap> Overlaps { get; set; }
    }

    public class SendEmailBulkOverlapStatusRequest_Overlap_Overlap
    {
        public string Teacher { get; set; }
        public string Building { get; set; }
        public string Venue { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public string Subject { get; set; }
    }
}
