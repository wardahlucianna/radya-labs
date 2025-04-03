using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservationSettings.MasterDayRestriction
{
    public class GetListMasterDayRestrictionRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
