using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class RedisAttendanceSummaryAttendanceMappingAttendanceResult
    {
        public string Id { get; set;}
        public AbsenceCategory? AbsenceCategory { get; set; }
    }
}
