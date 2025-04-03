using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.Venue
{
    public class GetVenueForAscTimetableRequest
    {
        public List<string> VenueCode { get; set; }
        public string IdSchool { get; set; }
    }
}
