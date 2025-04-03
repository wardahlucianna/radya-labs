using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class RedisAttendanceSummaryPeriodResult
    {
        public string Id { get; set; }
        public string IdGrade { get; set; }
        public string IdLevel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime AttendanceStartDate { get; set; }
        public DateTime AttendanceEndDate { get; set; }
        public int Semester { get; set; }
        public string IdSchool { get; set; }
    }
}
