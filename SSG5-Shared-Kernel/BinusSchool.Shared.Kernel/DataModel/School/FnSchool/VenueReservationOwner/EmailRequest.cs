using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.VenueReservationOwner
{
    public class EmailRequest
    {
        public string OwnerEmail { get; set; }
        public bool IsOwnerEmailTo { get; set; }
        public bool IsOwnerEmailCC { get; set; }
        public bool IsOwnerEmailBCC { get; set; }
    }
}
