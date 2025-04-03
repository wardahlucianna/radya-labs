using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class ApsentAppointmentBookingParentRequest
    {
        public  List<Absent> Absents { get; set; }
    }

    public class Absent
    {
        public string IdInvitationBooking { get; set; }
        public InvitationBookingStatus Status { get; set; }
        public string Note { get; set; }
    }
}
