using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.LevelByPosition
{
    public class LevelByPositionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string SelectedPosition { get; set; }
    }
}
