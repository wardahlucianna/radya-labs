using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation
{
    public class GetPersonalInvitationResult : CodeWithIdVm
    {
        public string InvitationDate { get; set; }  
        public string TeacherName { get; set; }  
        public string BinusianId { get; set; }  
        public string StartTime { get; set; }  
        public string EndTime { get; set; }  
        public string Status { get; set; }  
        public string StudentName { get; set; }  
        public string SendInvitationTo { get; set; }  
        public string InvitationType { get; set; }  
    }
}

