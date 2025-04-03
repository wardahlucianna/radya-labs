using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation
{
    public class UpdatePersonalInvitationRequest
    {
        public string Id { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string IdStudent { get; set; }
        public string IdUserTeacher { get; set; }
        public string Role { get; set; }
        public bool SendInvitationIsStudent { get; set; }
        public bool SendInvitationIsMother { get; set; }
        public bool SendInvitationIsFather { get; set; }
        public bool SendInvitationIsBothParent { get; set; }
        public bool IsNotifParent { get; set; }
        public DateTime InvitationDate { get; set; }
        public string InvitationStartTime { get; set; }
        public string InvitationEndTime { get; set; }
        public string Description { get; set; }
        public PersonalInvitationType? InvitationType { get; set; }
    }
}
