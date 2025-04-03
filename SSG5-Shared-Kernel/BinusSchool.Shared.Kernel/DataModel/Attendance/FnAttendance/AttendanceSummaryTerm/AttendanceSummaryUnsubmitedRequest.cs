using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class AttendanceSummaryUnsubmitedRequest
    {
        public List<RedisAttendanceSummaryScheduleLessonResult> ListScheduleLesoon { get; set; }
        //public List<RedisAttendanceSummaryPeriodResult> ListPeriod { get; set; }
        public List<RedisAttendanceSummaryStudentStatusResult> ListStaudetStatus { get; set; }
        public List<GetHomeroom> ListHomeroomStudentEnrollment { get; set; }
        public List<RedisAttendanceSummaryAttendanceEntryResult> ListAttendanceEntry { get; set; }
        public List<RedisAttendanceSummaryScheduleResult> ListSchedule { get; set; }
        public List<RedisAttendanceSummaryMappingAttendanceResult> ListMappingAttendance { get; set; }
        public List<RedisAttendanceSummaryLessonTeacherResult> ListLessonTeacher { get; set; }
        public List<RedisAttendanceSummaryHomeroomTeacherResult> ListHomeroomTeacher { get; set; }
        public List<GetHomeroom> ListTrHomeroomStudentEnrollment { get; set; }
        public string SelectedPosition { get; set; }
    }
}
