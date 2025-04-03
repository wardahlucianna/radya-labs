using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition
{
    public class HomeroomByPositionResult
    {
        public string IdUser { get; set; }
        public string Posistion { get; set; }
        public HomeroomByPosition Homeroom {get; set;}
    }

    public class HomeroomByPosition
    {
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string Homeroom { get; set; }
        public int Semester { get; set; }
    }
}
