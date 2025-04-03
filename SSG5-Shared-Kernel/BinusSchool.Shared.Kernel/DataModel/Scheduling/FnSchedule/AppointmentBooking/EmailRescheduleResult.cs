using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class EmailRescheduleResult
    {
        public string IdInvitationBookingSetting { get; set; }
        public string IdInvitationBooking { get; set; }
        public string AcademicYear { get; set; }
        public string InvitationName { get; set; }
        public string InvitationDate { get; set; }
        public string StudentName { get; set; }
        public string BinusianId { get; set; }
        public string Venue { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string IdUserTeacher { get; set; }
        public string IdUserParent { get; set; }
        public string TeacherName { get; set; }
        public string BookByParent { get; set; }
        public string IdSchool { get; set; }
        /// <summary>
        /// Type = New, Old
        /// </summary>
        public string Type { get; set; }
    }
}
