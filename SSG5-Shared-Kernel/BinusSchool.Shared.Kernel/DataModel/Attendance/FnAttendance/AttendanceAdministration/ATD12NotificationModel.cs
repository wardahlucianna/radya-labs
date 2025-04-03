using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration
{
    public class ATD12NotificationModel
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string AttendanceStatus { get; set; }
        public string AttendanceName { get; set; }
        public string Reason { get; set; }
        public List<SessionUsed> SessionUseds { get; set; }
    }
    public class SessionUsed
    {
        public string IdTeacher { get; set; }
        public string TeacherName { get; set; }
        public string ClassID { get; set; }
        public string SessionID { get; set; }
        public DateTime Date { get; set; }
    }
}
