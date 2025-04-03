using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class GetDetailBookingEquipmentOnlyResult
    {
        public string IdMappingEquipmentReservation { get; set; }
        public DateTime ScheduleDate { get; set; }
        public GetDetailBookingEquipmentOnlyResult_StartEndTime StartEndTime { get; set; }
        public NameValueVm Requester { get; set; }
        public string EventDescription { get; set; }
        public ItemValueVm Venue { get; set; }
        public string VenueNameinEquipment { get; set; }
        public string? Notes { get; set; }
        public List<ListEquipmentForBookingEquipmentOnly> ListEquipment { get; set; }
    }

    public class GetDetailBookingEquipmentOnlyResult_StartEndTime
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
