using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.VenueReservationOwner
{
    public class GetDDLVenueReservationOwnerRequest
    {
        public string IdSchool { get; set; }
        public bool? IsPICVenue { get; set; }
        public bool? IsPICEquipment { get; set; }
    }
}
