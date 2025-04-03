using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry
{
    public class UpdateAttendanceEntryRequest
    {
        /// <summary>
        /// IdHomeroom
        /// </summary>
        public string Id { get; set; }
        public string IdSchool { get; set; }
        public DateTime Date { get; set; }
        /// <summary>
        /// Fill this field from <see cref="BinusSchool.Common.Constants.PositionConstant"/>
        /// </summary>
        public string CurrentPosition { get; set; }
        public string ClassId { get; set; }
        public string IdSession { get; set; }
        /// <summary>
        /// Only work when absent term is Session
        /// </summary>
        public bool CopyToNextSession { get; set; }
        public bool SendLateEmailToParent { get; set; }
        public bool SendAbsentEmailToParent { get; set; }
        public IEnumerable<UpdateAttendanceEntryStudent> Entries { get; set; }
    }

    public class UpdateAttendanceEntryStudent
    {
        public string IdGeneratedScheduleLesson { get; set; }
        public string IdAttendanceMapAttendance { get; set; }
        public IEnumerable<string> IdWorkhabits { get; set; }
        public string LateInMinute { get; set; }
        public string File { get; set; }
        public string Note { get; set; }
    }
}
