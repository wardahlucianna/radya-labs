using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent
{
    public class AddCalendarEventRequest : CalendarEventIntendedForVm
    {
        public string IdSchool { get; set; }
        public string Name { get; set; }
        public IEnumerable<DateTimeRange> Dates { get; set; }
        public string IdEventType { get; set; }
        public EventAttendanceType Attendance { get; set; }
    }
}