using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class EmailInvitationBookingSettingResult
    {
        public string IdInvitationBookingSetting { get; set; }
        public string AcademicYear { get; set; }
        public string InvitationName { get; set; }
        public string InvitationStartDate { get; set; }
        public string InvitationEndDate { get; set; }
        public string ParentBookingStartDate { get; set; }
        public string ParentBookingEndDate { get; set; }
        public string StaffBookingStartDate { get; set; }
        public string StaffBookingEndDate { get; set; }
        public List<string> IdListTeacher { get; set; }
        public string IdSchool { get; set; }
        public string Link { get; set; }
    }
}
