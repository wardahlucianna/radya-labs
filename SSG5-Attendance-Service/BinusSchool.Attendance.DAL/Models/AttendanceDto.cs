using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Persistence.AttendanceDb.Models
{
    public class AttendanceDto
    {
        public string IdScheduleLesson { get; set; }
        public string IdAttendanceMappingAttendance { get; set; }
        public AttendanceEntryStatus Status { get; set; }
        public bool IsFromAttendanceAdministration { get; set; }
        public string PositionIn { get; set; }
        public string IdHomeroomStudent { get; set; }
        public DateTime? DateIn { get; set; }

        public List<AttendanceWorkhabitDto> Workhabits { get; set; }
    }

    public class AttendanceWorkhabitDto
    {
        public string IdEntryWorkhabit { get; set; }
        public string IdMappingAttendanceWorkHabit { get; set; }
    }
}
