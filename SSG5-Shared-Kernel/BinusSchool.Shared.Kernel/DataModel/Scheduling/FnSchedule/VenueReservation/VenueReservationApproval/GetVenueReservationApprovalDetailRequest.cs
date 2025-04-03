using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval
{
    public class GetVenueReservationApprovalDetailRequest
    {
        public string IdUser { get; set; }
        public string IdBooking { get; set; }
    }
}
