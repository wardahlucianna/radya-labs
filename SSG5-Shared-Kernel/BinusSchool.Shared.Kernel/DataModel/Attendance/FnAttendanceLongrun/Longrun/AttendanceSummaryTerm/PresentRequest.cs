using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;

namespace BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm
{
    public class PresentRequest : DailyAttendanceReportDefaultRequest
    {
        public List<string> IdLessonUser { get; set; }
        public List<RedisAttendanceSummaryStudentStatusResult> ListStudentStatus { get; set; }
        public List<GetMappingAttendance> ListMappingAttendance { get; set; }
        public List<GetHomeroom> ListStudentEnrollmentUnion { get; set; }
        public List<GetTappingResult> ListTapping { get; set; }
    }
}
