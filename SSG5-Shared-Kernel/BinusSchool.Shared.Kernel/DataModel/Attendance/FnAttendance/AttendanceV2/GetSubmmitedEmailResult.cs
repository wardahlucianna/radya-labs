using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetSubmmitedEmailResult
    {
        public List<string> idUserRecepient { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string ClassId { get; set; }
        public string Week { get; set; }
        public string SubjectId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public List<AttendanceStudent> AttendanceEntry { get; set; }
    }

    public class AttendanceStudent
    {
        public string StudentName { get; set; }
        public string StudentId { get; set; }
        public string AttendanceName { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
