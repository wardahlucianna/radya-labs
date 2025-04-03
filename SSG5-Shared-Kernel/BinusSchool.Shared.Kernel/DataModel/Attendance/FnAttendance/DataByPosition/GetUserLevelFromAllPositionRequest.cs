using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition
{
    public class GetUserLevelFromAllPositionRequest
    {
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
    }
}
