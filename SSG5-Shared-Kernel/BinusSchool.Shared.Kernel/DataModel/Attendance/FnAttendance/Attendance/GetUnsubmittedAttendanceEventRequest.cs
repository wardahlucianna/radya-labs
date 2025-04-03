using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class GetUnsubmittedAttendanceEventRequest
    {
        public string idSchool { get; set; }
        public string idAcademicYear { get; set; }
        public string idUser { get; set; }
    }
}
