using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetEmergencyAttendanceSummaryRequest
    {
        public DateTime Date {  get; set; }
        public string IdAcademicYear {  get; set; }
        public string IdLevel { get; set; }
    }
}
