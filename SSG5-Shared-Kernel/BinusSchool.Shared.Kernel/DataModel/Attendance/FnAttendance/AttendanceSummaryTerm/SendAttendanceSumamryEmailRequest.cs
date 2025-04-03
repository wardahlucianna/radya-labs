using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class SendAttendanceSumamryEmailRequest
    {
        public string IdUser { get; set; }
        public string Link { get; set; }
        public string IdScenario { get; set; }
        public string IdSchool { get; set; }
    }
}
