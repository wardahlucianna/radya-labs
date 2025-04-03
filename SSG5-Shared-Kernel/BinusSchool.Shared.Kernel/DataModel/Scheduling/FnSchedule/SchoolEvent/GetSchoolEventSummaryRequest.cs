using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventSummaryRequest : CollectionSchoolRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IntendedFor { get; set; }
        public string IdEvent { get; set; }
        public string IdActivity { get; set; }
        public string IdAward { get; set; }
        public string IdUser { get; set; }
    }
}
