using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Venue
{
    public class GetVenueResult : CodeWithIdVm
    {
        public int Capacity { get; set; }
        public string Building { get; set; }
        public string BuildingDesc { get; set; }
        public bool CanModified { get; set; }
    }
}
