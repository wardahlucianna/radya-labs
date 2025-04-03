using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation
{
    public class AddEmailInvitationRequest
    {
        public string IdInvitationBookingSetting { get; set; }
        public List<string> IdHomeroomStudent { get; set; }
        public InvitationBookingInitiateBy InitiateBy { get; set; }
        public bool IsSendEmail { get; set; }
    }
}
