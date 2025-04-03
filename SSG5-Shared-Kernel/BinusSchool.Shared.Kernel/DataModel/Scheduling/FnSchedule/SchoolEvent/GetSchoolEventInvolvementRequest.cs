using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventInvolvementRequest : CollectionSchoolRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string IdActivity { get; set; }
        public string IdAward { get; set; }
        public string IdUser { get; set; }
        public string IdStudent { get; set; }
    }
}
