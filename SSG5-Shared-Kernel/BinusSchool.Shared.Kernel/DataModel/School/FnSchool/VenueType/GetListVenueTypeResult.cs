using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.VenueType
{
    public class GetListVenueTypeResult
    {
        public string IdVenueType { get; set; }
        public string VenueTypeName { get; set; }
        public string? Description { get; set; }
        public bool CanDelete { get; set; }
    }
}
