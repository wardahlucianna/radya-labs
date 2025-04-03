using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class ATD11V2NotificationModel
    {
        public List<ATD11V2NotificationAttendance> AttendanceAdmin { get; set; }
        public string IdUserApproval { get; set; }
    }

    public class ATD11V2NotificationAttendance
    {
        public string Id { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Category { get; set; }
        public string AttendanceStatus { get; set; }
        public string File { get; set; }
        public string Note { get; set; }
        public string AbsentBy { get; set; }
        public string Url { get; set; }
    }
}
