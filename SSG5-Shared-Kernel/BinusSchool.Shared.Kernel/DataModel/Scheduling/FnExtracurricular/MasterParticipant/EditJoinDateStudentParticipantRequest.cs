using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class EditJoinDateStudentParticipantRequest
    {
        public List<EditRequest> EditRequestsList { get; set; }
        public string IdExtracurricular { get; set; }
    }

    public class EditRequest
    {
        public string IdStudent { get; set; }
        public DateTime JoinDate { get; set; }
    }
}
