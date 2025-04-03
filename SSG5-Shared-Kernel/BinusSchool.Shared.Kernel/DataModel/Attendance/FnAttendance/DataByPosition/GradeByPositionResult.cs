using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition
{
    public class GradeByPositionResult
    {
        public string IdUser { get; set; }
        public string Posistion { get; set; }
        public GradeByPosition Grade { get; set; }
    }

    public class GradeByPosition
    {
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string Grade { get; set; }
    }
}
