using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent
{
    public class GetCalendarEventResult : ItemValueVm
    {
        public string Name { get; set; }
        public IEnumerable<DateTimeRange> Dates { get; set; }
        public CalendarEventTypeVm EventType { get; set; }
        public string Role { get; set; }
        public EventAttendanceType Attendance { get; set; }
        public EventOptionType Option { get; set; }
    }
}