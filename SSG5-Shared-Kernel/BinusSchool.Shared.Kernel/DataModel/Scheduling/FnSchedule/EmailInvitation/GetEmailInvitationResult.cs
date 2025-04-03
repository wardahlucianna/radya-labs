using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation
{
    public class GetEmailInvitationResult : CodeWithIdVm
    {
        public string BinusianId { get; set; }
        public string StudentName { get; set; }
        public string InitiateBy { get; set; }
        public string InvitationName { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public string InvitationDate { get; set; }
        public string LastSendEmail { get; set; }
        public bool CanSendEmail { get; set; }
    }
}
