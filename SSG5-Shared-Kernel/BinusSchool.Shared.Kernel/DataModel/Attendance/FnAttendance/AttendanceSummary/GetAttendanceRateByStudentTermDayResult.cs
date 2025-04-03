using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetAttendanceRateByStudentTermDayResult
    {
        public int TotalDay { get; set; }
        public int Present { get; set; }
        public int ExcusedAbsent { get; set; }
        public int UnexcusedAbsent { get; set; }
        public int Late { get; set; }
        public double PresenceRate { get; set; }
        public double PunctualityRate { get; set; }
    }
}
