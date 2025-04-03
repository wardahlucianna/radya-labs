using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;

namespace BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm
{
    public class SheetRequest : DefaultSheetRequest
    {
        public DailyAttendanceReportRequest Body { get; set; }
        public List<string> ListIdLessonUser { get; set; }
        public List<GetPrasentResult> ListPresent { get; set; }
        public List<RedisAttendanceSummaryStudentStatusResult> ListStudentStatus { get; set; }
        public List<GetTappingResult> ListTapping { get; set; }
        public byte[] Logo { get; set; }
        public DateTime Date { get; set; }
        public List<GetMappingAttendance> ListMappingAttendance { get; set; }
        public List<GetHomeroom> ListStudentEnrollmentUnion { get; set; }

    }
}
