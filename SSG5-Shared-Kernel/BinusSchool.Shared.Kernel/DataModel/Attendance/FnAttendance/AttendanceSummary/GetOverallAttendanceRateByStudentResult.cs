using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetOverallAttendanceRateByStudentResult
    {
        public int Semester { get; set; }
        public int? TotalSession { get; set; }
        public int? TotalSessionSubmitted { get; set; }
        public int? Present { get; set; }
        public int? ExcusedAbsent { get; set; }
        public int? UnexcusedAbsent { get; set; }
        public int? Late { get; set; }
        public decimal? PresenceRate { get; set; }
        public decimal? PunctualityRate { get; set; }
    }
}
