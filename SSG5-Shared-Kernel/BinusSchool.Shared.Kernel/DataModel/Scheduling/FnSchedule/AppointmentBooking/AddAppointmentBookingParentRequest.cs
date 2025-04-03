using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class AddAppointmentBookingParentRequest
    {
        public string IdInvitationBookingSetting { get; set; }
        public string IdVenue { get; set; }
        public string IdUserTeacher { get; set; }
        public string IdUser { get; set; }
        public DateTime StartDateTimeInvitation { get; set; }
        public DateTime EndDateTimeInvitation { get; set; }
        public bool IsAddAppoitmentSchedule { get; set; }
        public InvitationBookingInitiateBy InitiateBy { get; set; }
        public List<HomeroomStudents> IdHomeroomStudents { get; set; }
    }

    public class HomeroomStudents
    {
        public string IdHomeroomStudent { set; get; }
    }
}
