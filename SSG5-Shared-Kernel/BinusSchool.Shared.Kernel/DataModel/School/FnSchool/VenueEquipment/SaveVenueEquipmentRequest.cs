using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.VenueEquipment
{
    public class SaveVenueEquipmentRequest
    {
        public string IdVenueEquipment { get; set; }
        public string IdVenue { get; set; }
        public string IdEquipment { get; set; }
        public int EquipmentQty { get; set; }
    }

    public class SaveVenueEquipRequest
    {
        public int Status { get; set; }
        public List<SaveVenueEquipmentRequest> EquipmentList { get; set; }
    }
}
