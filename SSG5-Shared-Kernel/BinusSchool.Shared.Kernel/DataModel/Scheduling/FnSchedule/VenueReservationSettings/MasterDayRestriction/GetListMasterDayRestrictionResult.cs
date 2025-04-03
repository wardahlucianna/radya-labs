using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction
{
    public class GetListMasterDayRestrictionResult
    {
        public string IdGroupRestriction { get; set; }
        public DateTime StartRestrictionDate { get; set; }
        public DateTime EndRestrictionDate { get; set; }    
        public string RestrictionPlace { get; set; }
    }
}
