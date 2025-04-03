using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class CancelAttendanceRequest
    {
        public string IdAttendanceAdministration { get; set; }
        public string IdStudent { get; set; }
        public List<string> IdScheduleLessons { get; set; }
    }
}
