using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Persistence.AttendanceDb.Models
{
    public class Entry
    {
        public Entry()
        {
            Workhabits = new List<WorkHabit>();
        }

        /// <summary>
        /// Only use for get list
        /// </summary>
        public string IdScheduleLesson { get; set; }
        public string IdEntry { get; set; }
        public string IdAttendanceMappingAttendance { get; set; }
        public string IdAttendance { get; set; }
        public TimeSpan? LateTime { get; set; }
        public string PositionIn { get; set; }
        public AttendanceEntryStatus EntryStatus { get; set; }
        public List<WorkHabit> Workhabits { get; }
    }
}
