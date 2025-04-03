using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration
{
    public class ATD12V2NotificationModel
    {
        public List<ATD12V2NotificationAttendance> AttendanceAdmin { get; set; }
        public string IdUserHomeroomTeacher { get; set; }
        public string Email { get; set; }
    }
    public class ATD12V2NotificationAttendance
    {
        public string Id { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string Homeroom { get; set; }
        public string AttendanceName { get; set; }
        public string Date { get; set; }
        public string DateRange { get; set; }
        public string IdLevel { get; set; }
    }
}
