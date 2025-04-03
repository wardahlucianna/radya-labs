using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation
{
    public class GetPersonalInvitationApprovalResult : CodeWithIdVm
    {
        public string TeacherName { get; set; }
        public string StudentName { get; set; }
        public string BinusanId { get; set; }
        public string InvitationDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Status { get; set; }
        public bool DisableButton { get; set; }
    }
}
