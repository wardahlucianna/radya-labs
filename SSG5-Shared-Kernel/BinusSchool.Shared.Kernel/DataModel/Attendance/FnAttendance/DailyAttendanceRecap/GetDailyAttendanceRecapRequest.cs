using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.DailyAttendanceRecap
{
    public class GetDailyAttendanceRecapRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdBinusian { get; set; }
        public int?  Semester { get; set; }
        public string IdHomeroom { get; set; }
        public string IdGrade { get; set; }
        public string IdLevel { get; set; }
    }
}
