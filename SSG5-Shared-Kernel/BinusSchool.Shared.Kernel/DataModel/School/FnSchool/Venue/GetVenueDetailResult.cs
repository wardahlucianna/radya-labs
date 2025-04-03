using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Venue
{
    public class GetVenueDetailResult : DetailResult2
    {
        public CodeWithIdVm Building { get; set; }
        public int Capacity { get; set; }
        public bool CanEditName { get; set; }
    }
}
