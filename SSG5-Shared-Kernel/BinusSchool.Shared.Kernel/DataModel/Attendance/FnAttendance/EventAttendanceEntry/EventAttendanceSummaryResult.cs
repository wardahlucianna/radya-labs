using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry
{
    public class EventAttendanceSummaryResult
    {
        public ItemValueVm EventCheck { get; set; }
        public CodeWithIdVm Level { get; set; }
        public EventAttendanceEntrySummary Summary { get; set; }
        public IEnumerable<EventAttendanceSummaryStudent> Entries { get; set; }
    }

    public class EventAttendanceSummaryStudent : NameValueVm
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string IdUserEvent { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Pathway { get; set; }
        public AttendanceEntryItem Attendance { get; set; }
    }
}
