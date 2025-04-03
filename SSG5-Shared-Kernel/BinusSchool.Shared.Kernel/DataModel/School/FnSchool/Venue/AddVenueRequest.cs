using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Venue
{
    public class AddVenueRequest : CodeVm
    {
        public string IdBuilding { get; set; }
        public string CapacityValue { get; set; }
        public int Capacity { get; set; }
    }
}
