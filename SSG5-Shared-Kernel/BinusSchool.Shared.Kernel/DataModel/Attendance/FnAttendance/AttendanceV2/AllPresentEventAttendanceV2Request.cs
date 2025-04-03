using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class AllPresentEventAttendanceV2Request
    {
        public string IdEvent { get; set; }
        public string IdUser { get; set; }
        public DateTime Date { get; set; }
    }
}
