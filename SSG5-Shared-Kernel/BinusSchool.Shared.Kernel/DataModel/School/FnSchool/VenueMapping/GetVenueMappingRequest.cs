using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.VenueMapping
{
    public class GetVenueMappingRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdSchool { get; set; }
        public string? IdBuilding { get; set; }

    }
}
