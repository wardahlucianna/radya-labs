using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetOverallTermAttendanceRateByStudentRequest
    {
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Term { get; set; }
    }
}
