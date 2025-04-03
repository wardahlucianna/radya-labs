using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class CreateInvitationBookingSettingRequest
    {
        public int StepWizard { get; set; }
        public GeneralInfo GeneralInfo { get; set; }
        public List<UserVenueMapping> UserVenueMapping { get; set; }
        public List<SettingQuotaDuration> SettingQuotaDuration { get; set; }
    }

    public class GeneralInfo
    {
        public string IdInvitationBookingSetting { get; set; }
        public string IdAcademicYear { get; set; }
        public string InvitationName { get; set; }
        public DateTime InvitationStartDate { get; set; }
        public DateTime InvitationEndDate { get; set; }
        public InvitationType InvitationType { get; set; }
        public DateTime ParentBookingStartDate { get; set; }
        public DateTime ParentBookingEndDate { get; set; }
        public DateTime? StaffBookingStartDate { get; set; }
        public DateTime? StaffBookingEndDate { get; set; }
        public bool SchedulingSiblingSameTime { get; set; }
        public string FootNote { get; set; }
        public List<string> IdHomeroom { get; set; }
        public List<string> IdGrade { get; set; }
        public List<string> PersonalIdStudent { get; set; }
        public bool AllGrade { get; set; }
        public List<RoleParticipant> RoleParticipants { get; set; }
        public List<ExcludeSubject> ExcludeSubjects { get; set; }
    }

    public class RoleParticipant
    {
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
    }

    public class ExcludeSubject
    {
        public string IdGrade{ get; set; }
        public string IdSubject { get; set; }
    }

    public class UserVenueMapping
    {
        public string IdInvitationBookingSettingVenueDate { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public List<DateTime> InvitationDate { get; set; }
        public List<DataUserVenue> DataUserVenue { get; set;}
    }

    public class DataUserVenue
    {
        public string IdUserTeacher { get; set; }
        public string IdVenue { get; set; }
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
    }

    public class SettingQuotaDuration
    {
        public string IdInvitationBookingSettingQuota { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public int SettingType { get; set; }
        public string IdGrade { get; set; }
        public DateTime DateInvitation { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? BreakBetweenSession { get; set; }
        public int QuotaSlot { get; set; }
        public int Duration { get; set; }
    }
}
