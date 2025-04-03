using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetEmergencyAttendanceSummaryResult
    {
        public string IdEmergencyReport { get; set; }
        public List<GetEmergencyAttendanceSummary_LevelVm> LevelSummarys { get; set; }
    }
    public class GetEmergencyAttendanceSummary_LevelVm
    {
        public CodeWithIdVm level { get; set; }
        public int TotalStudent { get; set; }
        public int TotalMarked { get; set; }
        public int TotalUnmarked { get; set; }
    }
}
