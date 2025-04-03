using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval
{
    public class GetVenueReservationApprovalListRequest
    {
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public DateTime BookingStartDate { get; set; }
        public DateTime BookingEndDate { get; set; }
        public string IdVenue { get; set; }
        public VenueApprovalStatus? BookingStatus { get; set; }
    }
}
