using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.VenueEquipment
{
    public class GetListVenueEquipmentResult
    {
        public string Building { get; set; }
        public string Venue { get; set; }
        public string IdVenue { get; set; }
        public int TotalEquipment { get; set; }
    }
}
