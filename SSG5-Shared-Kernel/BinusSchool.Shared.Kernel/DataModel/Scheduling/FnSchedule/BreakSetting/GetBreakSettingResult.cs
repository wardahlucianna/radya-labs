using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.BreakSetting
{
    public class GetBreakSettingResult
    {
        public string IdInvitationBookingSettingSchedule { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public string InvitationName { get; set; }
        public bool IsAvailable { get; set; }
        public bool DisabledAvailable { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public bool IsFullBook { get; set; }
        public int Quota { get; set; }
        public List<Student> Students { get; set; }
        public bool IsPriority { get; set; }
        public bool DisabledPriority { get; set; }
        public List<BreakSettings> BreakSettings { get; set; }

        /// <summary>
        /// InvitationBooking
        /// FixedBreak
        /// FlexibleBreak
        /// PersonalInvitation
        /// AvailabilityBooking
        /// </summary>
        public string Type { get; set; }
        
    }

    public class Student
    {
        public string IdInvitationBooking { get; set; }
        public string IdStudent { get; set; }
        public string BinusianID { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string StudentName { get; set; }
        public string TeacherName { get; set; }
        public string IdUserTeacher { get; set; }
        public string Venue { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public string InitiateBy { get; set; }
        public bool DisableButton { get; set; }
    }

    public class BreakSettings
    {
        public string IdInvitationBookingSettingBreak { get; set; }
        public bool IsChecked { get; set; }
        public string Description { get; set; }
        public bool Disabledcheckbox { get; set; }
    }
}
