using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetEmergencyAttendancePrivilegeRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
    }
}
