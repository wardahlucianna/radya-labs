using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.VenueEquipment
{
    public class GetVenueEquipmentDetailResult
    {
        public List<VenueEquipmentDetail> VenueEquipmentDetails { get; set; }
        public string IdVenue { get; set; }
        public string VenueName { get; set; }
        public string IdBuilding { get; set; }
        public string BuildingName { get; set; }

    }

    public class VenueEquipmentDetail
    {
        public string IdVenueEquipment { get; set; }
        public string EquipmentType { get; set; }
        public string Owner { get; set; }
        public int EquipmentQty { get; set; }
        public string IdEquipment { get; set; }
        public string EquipmentName { get; set; }
    }
}
