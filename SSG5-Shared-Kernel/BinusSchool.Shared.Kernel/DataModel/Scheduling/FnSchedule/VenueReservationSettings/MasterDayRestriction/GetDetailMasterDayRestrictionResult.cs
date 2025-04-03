using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction
{
    public class GetDetailMasterDayRestrictionResult
    {
        public string IdGroupRestriction { get; set; }
        public DateTime StartRestrictionDate { get; set; }
        public DateTime EndRestrictionDate { get; set; }
        public List<CodeWithIdVm> ListBuilding { get; set; }
        public List<ItemValueVm> ListVenue { get; set; }
    }
}
