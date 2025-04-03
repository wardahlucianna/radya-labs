using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class CreateSchoolEventInvolvementRequest
    {
        public string EventName { get; set; }
        public string IdAcadyear { get; set; }
        public string IdEventType { get; set; }
        public IEnumerable<DateTimeRange> Dates { get; set; }
        public List<SchoolEventInvolvementActivity> Activity { get; set; }
    }

    public class SchoolEventInvolvementActivity
    {
        public string Id { get; set; }
        public string IdActivity { get; set; }
        public List<EventInvolvementActivityAward> EventActivityAwardIdUser { get; set; }
    }

    public class EventInvolvementActivityAward
    {
        public string IdStudent { get; set; }
        public List<string> IdAward { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public decimal Filesize { get; set; }
        public string OriginalFilename { get; set; }
    }
}
