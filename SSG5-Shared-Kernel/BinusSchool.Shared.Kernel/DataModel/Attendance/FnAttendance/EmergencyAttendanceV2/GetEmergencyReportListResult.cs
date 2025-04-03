using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetEmergencyReportListResult
    {
        public string idEmergencyReport { get; set; }
        public CodeWithIdVm academicYear { get; set; }
        public string startBy { get; set; }
        public string startedDate { get; set; }
        public string reportedBy { get; set; }
        public string reportedDate { get; set; }
    }
}
