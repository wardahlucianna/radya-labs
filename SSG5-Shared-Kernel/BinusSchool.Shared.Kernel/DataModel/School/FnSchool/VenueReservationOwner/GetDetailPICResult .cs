using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.VenueReservationOwner
{
    public class GetDetailPICResult 
    {
        public string IdReservationOwner { get; set; }
        public string OwnerName { get; set; }
        public bool IsPICVenue { get; set; }
        public bool IsPICEquipment { get; set; }
        public bool VenueMapping { get; set; }
        public List<EmailRequest> PICEmail { get; set; }
    }
}
