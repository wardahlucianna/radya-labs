using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class GetListBookingEquipmentOnlyResult
    {
        public string IdMappingEquipmentReservation { get; set; }
        public DateTime ScheduleDate { get; set; }
        public GetListBookingEquipmentOnlyResult_StartEndTime StartEndTime { get; set; }
        public NameValueVm Requester { get; set; }
        public string EventDescription { get; set; }
        public ItemValueVm Venue { get; set; }
        public List<GetListBookingEquipmentOnlyResult_Equipment> ListEquipment { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public string VenueNameinEquipment { get; set; }
    }

    public class GetListBookingEquipmentOnlyResult_StartEndTime
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class GetListBookingEquipmentOnlyResult_Equipment
    {
        public string IdEquipment { get; set; }
        public string EquipmentName { get; set; }
        public int EquipmentBorrowingQty { get; set; }
    }
}
