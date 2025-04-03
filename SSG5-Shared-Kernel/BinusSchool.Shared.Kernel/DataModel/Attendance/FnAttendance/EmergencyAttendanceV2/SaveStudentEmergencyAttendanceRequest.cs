using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class SaveStudentEmergencyAttendanceRequest
    {
        public string IdAcademicYear { get; set; }
        public string RequestBy {  get; set; }
        public List<SaveStudentEmergencyAttendance_student> studentList {  get; set; }

    }

    public class SaveStudentEmergencyAttendance_student
    {
        public string IdEmergencyAttendance { get; set; }
        public string IdStudent {  get; set; }
        public string IdEmergencyStatus { get; set; }
        public string Description { get; set; }

    }
}
