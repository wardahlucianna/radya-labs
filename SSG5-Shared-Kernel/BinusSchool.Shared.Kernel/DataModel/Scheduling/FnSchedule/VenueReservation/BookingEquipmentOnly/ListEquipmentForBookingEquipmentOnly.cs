using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class ListEquipmentForBookingEquipmentOnly
    {
        public string IdEquipment { get; set; }
        public string EquipmentName { get; set; }
        public string EquipmentType { get; set; }
        public NameValueVm Owner { get; set; }
        public int CurrentAvailableStock { get; set; }
        public int? MaxBorrowingQty { get; set; }
        public int EquipmentBorrowingQty { get; set; }
    }
}
