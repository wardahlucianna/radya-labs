using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class ExportExcelVenueAndEquipmentReservationSummaryRequest
    {
        public DateTime BookingStartDate { get; set; }
        public DateTime BookingEndDate { get; set; }
        public string IdBuilding { get; set; }
        public string IdVenue { get; set; }
        public VenueApprovalStatus? ApprovalStatus { get; set; }
    }
}
