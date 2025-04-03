using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.VenueEquipment
{
    public class GetListVenueEquipmentRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string? Building {  get; set; }
        public string? Venue { get; set; }
        public string? SearchKey { get; set; }
    }
}
