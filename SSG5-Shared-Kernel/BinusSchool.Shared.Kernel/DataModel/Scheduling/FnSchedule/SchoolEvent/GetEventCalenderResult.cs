using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetEventCalendarResult : ItemValueVm
    {
        public string Name { get; set; }
        public IEnumerable<DateTimeRange> Dates { get; set; }
        public CalendarEventTypeVm EventType { get; set; }
        public IEnumerable<IntendedFor> IntendedFor { get; set; }
    }

    public class IntendedFor
    {
        public string Role { get; set; }
        public string Option { get; set; }
        public IEnumerable<string> Detail
        {
            get; set;
        }
    }
}
