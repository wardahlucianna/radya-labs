using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetStudentAttendanceSummaryTermRequest
    {
        public List<string> Students { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string Term { get; set; }
    }
}
