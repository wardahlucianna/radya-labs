using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;


namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetUserForUserVenueRequest : CollectionRequest
    {
        public string IdRole { get; set; }
        public string CodePosition { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdInvitationBookingSetting { get; set; }
    }
}
