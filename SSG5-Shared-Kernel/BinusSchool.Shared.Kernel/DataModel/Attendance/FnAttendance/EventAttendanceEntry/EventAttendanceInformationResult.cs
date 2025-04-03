using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry
{
    public class EventAttendanceInformationResult : ItemValueVm
    {
        public EventOptionType EventOptionType { get; set; }
        public NameValueVm PIC { get; set; }
        public List<EventDate> EventDates { get; set; }
        public List<DateTime> AvailableCheckDates { get; set; }
    }
}
