using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class ENS1Result
    {
        public string IdInvitationBookingSetting { get; set; }
        public string InvitationName { get; set; }
        public List<string> IdRecepient { get; set; }
        public List<ENS1InvitationBooking> InvitationBookingSettingOld { get; set; }
        public List<ENS1InvitationBooking> InvitationBookingSettingNew { get; set; }
    }

    public class ENS1InvitationBooking
    {
        public string AcademicYear { get; set; }
        public string InvitationName { get; set; }
        public string InvitationDate { get; set; }
        public string ParentBookingPeriod { get; set; }
        public string StaffBookingPeriod { get; set; }
        public string TeacherName { get; set; }
        public string Venue { get; set; }
        public string Link { get; set; }
    }
}
