using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class GetCancelAttendanceResult
    {
        public DateTime Date { get; set; }
        public List<ScheduleLessonCancel> ScheduleLessonCancels { get; set; }
        public bool IsDayDisabled { get; set; }
    }

    public class ScheduleLessonCancel
    {
        public DateTime Date { get; set; }
        public string IdScheduleLesson { get; set; }
        public string SessionId { get; set; }
        public bool IsSessionDisabled { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
