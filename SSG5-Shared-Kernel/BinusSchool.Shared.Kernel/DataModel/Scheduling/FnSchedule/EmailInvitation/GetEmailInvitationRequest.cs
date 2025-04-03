using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation
{
    public class GetEmailInvitationRequest : CollectionSchoolRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string IdInvitationBookingSetting { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }

    }
}
