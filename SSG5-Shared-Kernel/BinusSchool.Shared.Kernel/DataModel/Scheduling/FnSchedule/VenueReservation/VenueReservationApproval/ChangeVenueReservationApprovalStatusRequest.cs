using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval
{
    public class ChangeVenueReservationApprovalStatusRequest
    {
        public string IdUser { get; set; }
        public string IdBooking { get; set; }
        public VenueApprovalStatus ApprovalStatus { get; set; }
        public string RejectionReason { get; set; }
    }
}
