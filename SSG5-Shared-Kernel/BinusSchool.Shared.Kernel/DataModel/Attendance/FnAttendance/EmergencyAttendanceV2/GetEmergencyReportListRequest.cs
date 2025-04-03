using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetEmergencyReportListRequest
    {
        public string IdAcademicYear { get; set; }
        public int? semester {  get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
       
    }
}
