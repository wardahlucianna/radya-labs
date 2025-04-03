using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetTeacherVenueMappingRequest
    {
        public string IdInvitationBookingSetting { get; set; }
        public DateTime InvitationDate { get; set; }
        public List<string> IdHomeroomStudents { get; set; }
    }
}
