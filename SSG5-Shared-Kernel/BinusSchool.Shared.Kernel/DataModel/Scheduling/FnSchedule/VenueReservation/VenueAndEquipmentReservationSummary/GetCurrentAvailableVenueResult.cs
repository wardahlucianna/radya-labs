using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class GetCurrentAvailableVenueResult
    {
        public ItemValueVm Building { get; set; }
        public ItemValueVm Venue { get; set; }
        public NameValueVm PicOwner { get; set; }
    }
}
