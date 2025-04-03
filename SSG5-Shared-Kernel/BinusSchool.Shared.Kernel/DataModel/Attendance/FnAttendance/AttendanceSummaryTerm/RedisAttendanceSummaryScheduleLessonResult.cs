using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class RedisAttendanceSummaryScheduleLessonResult
    {
        public string Id { get; set; }
        public DateTime ScheduleDate{ get; set; }
        public int Semester { get; set; }
        public string IdLesson { get; set; }
        public string ClassID { get; set; }
        public RedisAttendanceSummarySession Session { get; set; }
        public string IdGrade { get; set; }
        public string IdDay { get; set; }
        public string IdWeek { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public RedisAttendanceSummarySubject Subject { get; set; }
    }

    
}
