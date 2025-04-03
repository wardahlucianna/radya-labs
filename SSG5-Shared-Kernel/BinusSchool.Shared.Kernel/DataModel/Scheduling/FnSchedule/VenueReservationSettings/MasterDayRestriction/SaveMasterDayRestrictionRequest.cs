using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction
{
    public class SaveMasterDayRestrictionRequest
    {
        public string? IdGroupRestriction { get; set; }
        public string IdSchool { get; set; }
        public DateTime StartRestrictionDate { get; set; }
        public DateTime EndRestrictionDate { get; set; }
        public List<string> IdBuilding { get; set; }
        public List<string> IdVenue { get; set; }
    }
}
