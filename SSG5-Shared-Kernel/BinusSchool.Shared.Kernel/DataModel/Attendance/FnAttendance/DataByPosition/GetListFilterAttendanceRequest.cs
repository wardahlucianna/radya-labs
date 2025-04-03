using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition
{
    public class GetListFilterAttendanceRequest
    {
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public bool ShowLevel { get; set; }
        public bool ShowGrade { get; set; }
        public bool ShowSemester { get; set; }
        public bool ShowTerm { get; set; }
    }
}
