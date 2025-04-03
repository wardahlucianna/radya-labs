using System;
using System.Collections.Generic;

namespace BinusSchool.Attendance.FnAttendance.Models
{
    public class ScheduleLessonResult
    {
        public ScheduleLessonResult()
        {
            LessonPathwayResults = new List<ScheduleLessonPathwayResult>();
        }

        public string Id { get; set; }
        public DateTime ScheduleDate { get; set; }
        public int Semester { get; set; }
        public string IdLesson { get; set; }
        public string ClassID { get; set; }
        public AttendanceSummarySessionResult Session { get; set; }
        public string IdGrade { get; set; }
        public string IdDay { get; set; }
        public string IdWeek { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public AttendanceSummarySubjectResult Subject { get; set; }
        public List<ScheduleLessonPathwayResult> LessonPathwayResults { get; set; }
    }

    public class ScheduleLessonPathwayResult
    {
        public string IdHomeroom { get; set; }
    }
}
