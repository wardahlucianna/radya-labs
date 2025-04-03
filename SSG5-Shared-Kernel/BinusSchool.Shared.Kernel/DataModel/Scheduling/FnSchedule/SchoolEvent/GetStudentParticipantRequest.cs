using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetStudentParticipantRequest : CollectionSchoolRequest
    {
        public string IdEvent { get; set; }
        public string IdActivity { get; set; }
        public string IdUser { get; set; }
    }
}
