using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class RedisAttendanceSummaryLessonTeacherResult
    {
        public string IdLesson { set; get; }
        public string IdUserTeacher { set; get; }
        public string ClassId { set; get; }
        public bool IsAttendance { set; get; }
    }
}
