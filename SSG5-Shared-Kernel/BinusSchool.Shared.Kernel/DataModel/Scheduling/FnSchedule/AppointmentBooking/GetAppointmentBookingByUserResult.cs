using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAppointmentBookingByUserResult
    {
        public string IdInvitationBookingSetting { get; set; }
        public string InvitationBookingName { get; set; }
        public bool IsSchedulingSiblingSameTime { get; set; }
    }
}
