using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceRecap
{
    public class GetDetailHeaderAttendanceRecapResult
    {
        public string IdAcademicYear { get; set; }
        public string AcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdHomeroom { get; set; }
        public string Homeroom { get; set; }
        public string IdLevel {  get; set; }
        public string IdGrade {  get; set; }
    }
}
