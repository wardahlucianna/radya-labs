using System;

namespace BinusSchool.Attendance.FnAttendance.Models
{
    public class PeriodResult
    {
        public string IdPeriod { get; set; }
        public string IdGrade { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Semester { get; set; }
        public string IdLevel { get; set; }
        public DateTime AttendanceStartDate { get; set; }
        public DateTime AttendanceEndDate { get; set; }
    }
}
