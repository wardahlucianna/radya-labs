using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation
{
    public class DetailPersonalInvitationResult
    {
        public string Id { get; set; }  
        public string IdAcademicYear { get; set; }  
        public string IdUserTeacher { get; set; }  
        public string IdStudent { get; set; }  
        public string StudentName { get; set; }  
        public string TeacherName { get; set; }  
        public string Description { get; set; }  
        public string InvitationType { get; set; }  
        public string Status { get; set; }  
        public string Reason { get; set; }  
        public string SendInvitationTo { get; set; }  
        public bool MakeAppointmentWithIsStudent { get; set; }  
        public bool MakeAppointmentWithIsMother { get; set; }  
        public bool MakeAppointmentWithIsFather { get; set; }  
        public bool MakeAppointmentWithIsBothParent { get; set; }  
        public bool IsNotifParent { get; set; }  
        public DateTime InvitationStartDate { get; set; }  
        public DateTime InvitationEndDate { get; set; }  
        public bool IsDisabledReschedule{ get; set; }  
        public bool IsDisabledCancel{ get; set; }  
        public DateTime? AvailabilityDate { get; set; }  
        public TimeSpan? AvailabilityStartTime { get; set; }  
        public TimeSpan? AvailabilityEndTime { get; set; }  
    }
}
