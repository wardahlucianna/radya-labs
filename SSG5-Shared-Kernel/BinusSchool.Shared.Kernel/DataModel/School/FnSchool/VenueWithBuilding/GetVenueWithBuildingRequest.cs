using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.VenueWithBuilding
{
    public class GetVenueWithBuildingRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string? IdBuilding { get; set; }
    }
}
