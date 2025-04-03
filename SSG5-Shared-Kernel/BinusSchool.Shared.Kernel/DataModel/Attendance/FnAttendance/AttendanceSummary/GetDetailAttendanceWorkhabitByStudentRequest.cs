using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDetailAttendanceWorkhabitByStudentRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdStudent { get; set; }
        public string IdMappingAttendanceWorkhabit { get; set; }
    }
}
