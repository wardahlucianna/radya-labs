using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetStudentEmergencyAttendanceRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdScheduleLesson { get; set; }
        public string Status { get; set; }
        public string IdEmergencyStatus { get; set; }
    }
}
