using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.DailyAttendanceRecap
{
    public class GetDailyAttendanceRecapResult
    {
        public string Class { get; set; }
        public string IdBinusian { get; set; }
        public string HomeroomTeacher { get; set; }
        public int TotalUnsubmitted { get; set; }
        public List<string> UnsubmittedDate { get; set; }
    }

    public class DailyAbsentTermResult
    {
        public string IdLevel { get; set; }
    }

    public class ScheduleLessonResult
    {
        public string IdScheduleLesson { get; set; }
        public string IdLesson { get; set; }
        public DateTime ScheduleDate { get; set; }
    }

    public class StudentEnrollmentResult
    {
        public string IdLesson { get; set; }
        public string Class { get; set; }
        public string IdStudent { get; set; }
        public string IdHomeroomStudent { get; set; }
    }

    public class StudentStatusResult
    {
        public string IdStudent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class HomeroomTeacherResult
    {
        public string IdBinusian { get; set; }
        public string HomeroomTeacher { get; set; }
        public string IdLesson { get; set; }
        public string IdHomeroom { get; set; }
    }

    public class AttendanceEntryResult
    {
        public string IdAttendanceEntry { get; set; }
        public string IdScheduleLesson { get; set; }
        public string IdHomeroomStudent { get; set; }
    }

    public class GroupScheduleLessonResult
    {
        public string IdStudent { get; set; }
        public string IdScheduleLesson { get; set; }
        public string IdLesson { get; set; }
        public string Class { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string IdHomeroomStudent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class UnsubmittedAttendanceResult
    {
        public string IdBinusian { get; set; }
        public string HomeroomTeacher { get; set; }
        public string Class { get; set; }
        public string IdHomeroom { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string IdHomeroomStudent { get; set; }

    }
}
