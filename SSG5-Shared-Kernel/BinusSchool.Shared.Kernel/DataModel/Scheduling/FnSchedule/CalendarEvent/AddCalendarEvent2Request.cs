using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent
{
    public class AddCalendarEvent2Request : CalendarEvent2IntendedForVm
    {
        public string Name { get; set; }
        public IEnumerable<DateTimeRange> Dates { get; set; }
        public string IdEventType { get; set; }
    }
}