using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class ExportExcelEmergencyReportDetailRequest
    {
        public string idEmergencyReport { get; set; }
        public string idLevel { get; set; }
        public string idGrade { get; set; }
        public string idHomeroom { get; set; }
        public string IdEmergencyStatus { get; set; }
    }
}
