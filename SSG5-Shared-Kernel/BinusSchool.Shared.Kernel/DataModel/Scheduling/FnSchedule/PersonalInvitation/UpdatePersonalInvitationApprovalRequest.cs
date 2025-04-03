using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation
{
    public class UpdatePersonalInvitationApprovalRequest
    {
        public string Id { set; get; }
        /// <summary>
        /// Role : PARENT,TEACHER
        /// </summary>
        public string Role { set; get; }
        public bool IsApproval { set; get; }
        public string IdVanue { set; get; }
        public DateTime? StartDateAvailability { set; get; }
        public DateTime? EndDateAvailability { set; get; }
        public string Note { set; get; }

    }
}
