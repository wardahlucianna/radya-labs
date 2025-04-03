using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration
{
    public class GetAttendanceAdministrationSummaryRequest
    {
        public bool IsAllDay {get;set;}
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string IdAttendance { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartPeriod { get; set; }
        public TimeSpan EndPeriod { get; set; }
    }
}
