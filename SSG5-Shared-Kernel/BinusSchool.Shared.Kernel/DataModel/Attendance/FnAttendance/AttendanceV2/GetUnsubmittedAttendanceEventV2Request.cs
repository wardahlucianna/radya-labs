using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetUnsubmittedAttendanceEventV2Request
    {
        public string idSchool { get; set; }
        public string idAcademicYear { get; set; }
        public string idUser { get; set; }
    }
}
