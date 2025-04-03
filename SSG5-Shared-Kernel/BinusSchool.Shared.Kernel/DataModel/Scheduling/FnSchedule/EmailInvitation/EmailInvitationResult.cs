using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation
{
    public class EmailInvitationResult
    {
        public string StudentName { get; set; }
        public string BinusianId { get; set; }
        public string InvitationName { get; set; }
        public string EarlyBook { get; set; }
        public string ParentBook { get; set; }
        public string Teacher { get; set; }
        public string Link { get; set; }
        public string IdTeacher { get; set; }
        public string IdParent { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public string DateInvitation { get; set; }
        public string IdSchool { get; set; }
        public bool IsSchedulingSameTime { get; set; }
    }
}
