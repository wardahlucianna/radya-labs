using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDashboardResult
    {
        public string BinusianId { get; set; }
        public string StudentName { get; set; }
        public string Homeroom { get; set; }
        public AbsentTerm Term { get; set; }
        public bool UseWorkhabit { get; set; }
        public double AttendanceRate { get; set; }
        public int TotalDay { get; set; }
        public List<AttendanceStudent> Attendances { get; set; }
        public List<WorkhabitStudent> Workhabits { get; set; }
        public DateTime ValidDate { get; set; }
        public string IdLevel { get; set; }
        public string IdAcademicYear { get; set; }
    }

    public class AttendanceStudent
    {
        public string IdAttendance { get; set; }
        public string AttendanceName { get; set; }
        public int Count { get; set; }
    }

    public class WorkhabitStudent
    {
        public string IdWorkhabit { get; set; }
        public string WorkhabitName { get; set; }
        public int Count { get; set; }
    }
}
