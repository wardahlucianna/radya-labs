using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation
{
    public class GetPersonalInvitationApprovalRequest : CollectionSchoolRequest
    {
        public string IdUser { set; get; }
        public string IdStudent { set; get; }
        public string IdAcademicYear { set; get; }
        /// <summary>
        /// Role : STUDENT,PARENT
        /// </summary>
        public string Role { set; get; }
        public DateTime? DateInvitation { set; get; }
    }
}
