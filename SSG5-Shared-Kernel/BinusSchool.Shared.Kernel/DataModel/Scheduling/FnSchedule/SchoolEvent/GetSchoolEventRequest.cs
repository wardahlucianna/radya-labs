using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolsEvent
{
    public class GetSchoolEventRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string IdEventType { get; set; }
        public string AssignedAs { get; set; }
        public string ApprovalStatus { get; set; }
        public string IdUser { get; set; }
    }
}
