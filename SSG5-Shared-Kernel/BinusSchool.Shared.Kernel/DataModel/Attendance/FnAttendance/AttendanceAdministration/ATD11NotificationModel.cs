using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration
{
    public class ATD11NotificationModel
    {
        public string BinussianId { get; set; }
        public string StudentName { get; set; }
        public string StudentGrade { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartSession { get; set; }
        public string EndSession { get; set; }
        public string Category { get; set; }
        public string Reason { get; set; }
    }
}
