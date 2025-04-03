using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry
{
    public class EventAttendanceEntryResult
    {
        public ItemValueVm EventCheck { get; set; }
        public CodeWithIdVm Level { get; set; }
        public EventAttendanceEntrySummary Summary { get; set; }
        public IEnumerable<EventAttendanceEntryStudent> Entries { get; set; }
        public EventIntendedForAttendanceStudent AttendanceType { get; set; }
    }

    public class EventAttendanceEntryStudent : NameValueVm
    {
        public string IdUserEvent { get; set; }
        public AttendanceEntryItem Attendance { get; set; }
        public IEnumerable<AttendanceEntryItemWorkhabit> Workhabits { get; set; }
        public TimeSpan? Late { get; set; }
        public int LateInMinute => Late.HasValue ? (int)Late.Value.TotalMinutes : 0;
        public string File { get; set; }
        public string Note { get; set; }
        public bool IsSubmitted { get; set; }
    }

    public class EventAttendanceEntrySummary
    {
        public int TotalStudent { get; set; }
        public int Submitted { get; set; }
        public int Unsubmitted => TotalStudent - Submitted;
    }
}
