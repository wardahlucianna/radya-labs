using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceEntryBySchoolDayResult
    {
        public string IdScheduleLesson { get; set; }
        public DateTime ScheduleDate { get; set; }
        public AttendanceEntryStatus Status { get; set; }
        public string IdAttendanceMappingAttendance { get; set; }
        public string IdHomeroomStudent { get; set; }
        public List<GetAttendanceEntryWorkhabitV2> AttendanceEntryWorkhabit { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string IdStudent { get; set; }
    }

    public class GetAttendanceEntryWorkhabitV2
    {
        public string IdAttendanceEntry { get; set; }
        public string IdMappingAttendanceWorkhabit { get; set; }
    }
}
