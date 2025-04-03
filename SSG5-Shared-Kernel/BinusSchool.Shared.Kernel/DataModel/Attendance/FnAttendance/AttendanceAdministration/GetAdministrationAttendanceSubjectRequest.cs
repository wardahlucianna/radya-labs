using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration
{
    public class GetAdministrationAttendanceSubjectRequest
    {
        public string Search { get; set; }
        public string IdHomeroom { get; set; }
        public string IdUserStudent { get; set; }
    }
}
