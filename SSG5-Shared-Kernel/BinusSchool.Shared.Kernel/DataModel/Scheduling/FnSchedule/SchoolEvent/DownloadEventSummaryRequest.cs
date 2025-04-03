using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class DownloadEventSummaryRequest : CollectionSchoolRequest
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string IntendedFor { get; set; }
        public string IdEvent { get; set; }
        public string IdActivity { get; set; }
        public string IdAward { get; set; }
        public string IdUser { get; set; }
    }
}
