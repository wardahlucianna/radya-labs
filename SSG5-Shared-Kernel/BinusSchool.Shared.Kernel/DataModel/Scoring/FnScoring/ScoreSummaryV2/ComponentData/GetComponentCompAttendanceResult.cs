using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentCompAttendanceResult
    {
        public List<GetComponentCompAttendanceResult_Attendance> Attendances { get; set; }
    }

    public class GetComponentCompAttendanceResult_Attendance
    {
        public int Semester { get; set; }
        public double AttendanceRate { get; set; }
    }
}
