using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class GetCurrentAvailableEquipmentStockResult
    {
        public NameValueVm EquipmentType { get; set; }
        public string EquipmentName { get; set; }
        public int? MaxQtyBorrowing { get; set; }
        public int CurrentAvailableStock { get; set; }
    }
}
