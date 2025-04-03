using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class UpdateInvitationBookingSettingVanueOnlyRequest
    {
        public string IdInvitationBookingSetting {  get; set; }
        public List<UserVenue> UserVenueMapping { get; set; }
    }

    public class UserVenue
    {
        public string IdInvitationBookingSettingVenueDtl { get; set; }
        public string IdUserTeacher { get; set; }
        public string IdVenue { get; set; }
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
    }
}
