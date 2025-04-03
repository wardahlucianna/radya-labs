using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class UpdateSchoolEventInvolvementRequest
    {
        public string IdEvent { get; set; }
        public string IdUser { get; set; }
        public string EventName { get; set; }
        public string IdAcadyear { get; set; }
        public string IdEventType { get; set; }
        public IEnumerable<DateTimeRange> Dates { get; set; }
        public List<SchoolEventInvolvementActivity> Activity { get; set; }
    }
}
