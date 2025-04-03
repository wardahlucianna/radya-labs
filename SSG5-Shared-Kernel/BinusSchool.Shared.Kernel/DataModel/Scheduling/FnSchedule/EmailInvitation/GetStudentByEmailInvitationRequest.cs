using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation
{
    public class GetStudentByEmailInvitationRequest
    {
        public string IdInvitationBookingSetting { get; set; }
        public string IdParent { get; set; }

        /// <summary>
        /// Role : Staff/parent
        /// </summary>
        public string Role { get; set; }
        public string IdUserTeacher { get; set; }
    }
}
