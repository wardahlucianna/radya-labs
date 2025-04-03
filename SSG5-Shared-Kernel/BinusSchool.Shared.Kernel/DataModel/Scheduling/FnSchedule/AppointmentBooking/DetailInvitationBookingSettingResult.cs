using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking
{
    public class DetailInvitationBookingSettingResult
    {
        public int StepWizard { get; set; }
        public GeneralInfoDetail GeneralInfoDetail { get; set; }
        public List<UserVenueMappingDetail> UserVenueMappingDetail { get; set; }
        public List<SettingQuotaDurationDetail> SettingQuotaDurationDetail { get; set; }
    }

    public class GeneralInfoDetail
    {
        public CodeWithIdVm AcademicYear { get; set; }
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
        public List<CodeWithIdVm> Homeroom { get; set; }
        public List<CodeWithIdVm> Grade { get; set; }
        public List<DataStudent> PersonalStudent { get; set; }
        public List<DetailRoleParticipant> RoleParticipants { get; set; }
        public List<DetailExcludeSubject> ExcludeSubjects { get; set; }

    }

    public class DetailRoleParticipant
    {
        public ItemValueVm Role { get; set; }
        public DetailTeacherPosition TeacherPosition { get; set; }
    }

    public class DetailExcludeSubject
    {
        public ItemValueVm Grade { get; set; }
        public List<ItemValueVm> Subject { get; set; }
    }

    public class DetailTeacherPosition : CodeWithIdVm
    {
        public string IdPosition { get; set; }
        public string IdTeacherPosition { get; set; }
    }

    public class UserVenueMappingDetail
    {
        public string IdInvitationBookingSettingVenueDate { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public DateTime DateInvitationExact { get; set; }
        public List<string> InvitationDate { get; set; }
        public List<DataUserTeacher> UserTeacher { get; set; }
    }

    public class SettingQuotaDurationDetail
    {
        public string IdInvitationBookingSettingQuota { get; set; }
        public int SettingType { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public DateTime DateInvitation { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? BreakBetweenSession { get; set; }
        public int QuotaSlot { get; set; }
        public int Duration { get; set; }
    }

    public class DataUserTeacher : CodeWithIdVm
    {
        public string IdInvitationBookingSettingVenueDtl { get; set; }
        public string IdRole { get; set; }
        public string Role { get; set; }
        public string IdTeacherPosition { get; set; }
        public string Position { get; set; }
        public CodeWithIdVm Venue { get; set; }
    }

    public class DataStudent : CodeWithIdVm
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string BinusianId { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
    }
}
