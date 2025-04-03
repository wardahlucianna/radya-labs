using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;

namespace BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm
{
    public class GetTappingRequest : DailyAttendanceReportDefaultRequest
    {
        public List<RedisAttendanceSummaryStudentStatusResult> ListStudentStatus { get; set; }
    }
}
