using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAppointmentBookingDateRequest
    {
        public string IdUser { get; set; }  
        public string IdInvitationBookingSetting { get; set; }  
        public string Role { get; set; }  
        public List<string> IdStudentHomerooms { get; set; }
        public string IdUserTeacher { get; set; }
    }
}
