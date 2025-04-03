using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.SendEmail
{
    public class SendEmailBulkVenueReservationApprovalRequest
    {
        public List<string> IdBooking { get; set; }
    }
}
