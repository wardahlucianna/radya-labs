using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.VenueType
{
    public class SaveVenueTypeRequest
    {
        public string? IdVenueType { get; set; }
        public string IdSchool { get; set; }
        public string VenueTypeName { get; set; }
        public string Description { get; set; }
    }
}
