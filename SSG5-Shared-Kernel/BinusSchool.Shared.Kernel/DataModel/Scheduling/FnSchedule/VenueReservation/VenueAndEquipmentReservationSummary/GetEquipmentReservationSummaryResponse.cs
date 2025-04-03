using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class GetEquipmentReservationSummaryResponse
    {
        public string IdEquipmentReservation { get; set; }
        public DateTime ScheduleDate { get; set; }
        public GetEquipmentReservationSummaryResponse_Time Time { get; set; }
        public ItemValueVm Requester { get; set; }
        public ItemValueVm Venue { get; set; }
        public string Event { get; set; }
        public string Notes { get; set; }
        public GetEquipmentReservationSummaryResponse_Equipment Equipment { get; set; }
    }

    public class GetEquipmentReservationSummaryResponse_Time
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }

    public class GetEquipmentReservationSummaryResponse_Equipment
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Owner { get; set; }
        public int BorrowingQty { get; set; }
    }
}
