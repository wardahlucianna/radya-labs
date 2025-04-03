using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetHomeroomStudent
    {
        public string IdHomeroomStudent { get; set; }
        public string IdStudent { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string BinusianID { get; set; }
        public string IdLesson { get; set; }
        public string IdLevel { get; set; }
        public bool IsDelete { get; set; }
        public DateTime Datein { get; set; }
    }
}
