using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class AttendanceSummaryCalculationRequest
    {
        public string IdSchool { get; set; }
        public DateTime Date { get; set; }
    }
}
