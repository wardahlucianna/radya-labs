using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm
{
    public class GetTappingResult : GetPrasentResult
    {
        public TimeSpan? DetectedDate { get; set; }
    }
}
