using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class UnresolvedEventAttendanceResult : ItemValueVm
    {
        public List<EventDate> Dates { get; set; }
        public List<EventCheck> EventCheck { get; set; }
    }

    public class EventDate
    {
        public DateTime StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan EndTime { get; set; }
    }
    public class EventCheck : ItemValueVm
    {
        public DateTime Date { get; set; }
        public TimeSpan AttendanceTime { get; set; }
    }
}
