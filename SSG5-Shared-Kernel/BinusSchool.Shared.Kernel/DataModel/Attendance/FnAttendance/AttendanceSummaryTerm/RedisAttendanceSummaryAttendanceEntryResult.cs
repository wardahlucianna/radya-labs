using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class RedisAttendanceSummaryAttendanceEntryResult
    {
        public string IdScheduleLesson { get; set; }
        public string IdHomeroomStudent { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string IdLesson { get; set; }
        public string ClassID { get; set; }
        public RedisAttendanceSummarySession Session { get; set; }
        public CodeWithIdVm Classroom { get; set; }
        public string IdGrade { get; set; }
        public string GradeCode { get; set; }
        public string IdDay { get; set; }
        public string IdWeek { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public RedisAttendanceSummarySubject Subject { get; set; }
        public string IdStudent { get; set; }
        public AttendanceEntryStatus Status { get; set; }
        public RedisAttendanceSummaryAttendance Attendance { get; set; }
        public string Notes { get; set; }
        public string IdUserTeacher { get; set; }
        public RedisAttendanceSummaryStudent Student { get; set; }
        public string IdAttendanceMappingAttendance { get; set; }
        public int Semester { get; set; }
        public List<GetAttendanceEntryWorkhabitV2> AttendanceEntryWorkhabit { get; set; }
        public string IdUserAttendanceEntry { get; set; }
        public bool IsFromAttendanceAdministration { get; set; }
    }

    public class RedisAttendanceSummaryAttendance : CodeWithIdVm
    {
        public AbsenceCategory? AbsenceCategory { get; set; }
        public ExcusedAbsenceCategory? ExcusedAbsenceCategory { get; set; }
    }
    public class RedisAttendanceSummarySubject : CodeWithIdVm
    {
        public string SubjectID { get; set; }
    }

    public class RedisAttendanceSummarySession : NameValueVm
    {
        public string SessionID { get; set; }
    }
}
