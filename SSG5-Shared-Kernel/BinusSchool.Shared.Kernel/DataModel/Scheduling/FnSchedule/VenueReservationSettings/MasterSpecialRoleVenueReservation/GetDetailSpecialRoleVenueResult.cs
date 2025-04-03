using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterSpecialRoleVenueReservation
{
    public class GetDetailSpecialRoleVenueResult
    {
        public string IdSpecialRoleVenue { get; set; }
        public ItemValueVm Role { get; set; }
        public int SpecialDurationBookingTotalDay { get; set; }
        public bool? CanOverrideAnotherReservation { get; set; }
        public bool? AllSuperAccess { get; set; }
    }
}
