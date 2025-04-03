using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetStudentAttendanceSummaryTermResult
    {
        public NameValueVm Student { get; set; }
        //public NameValueVm Homeroom { get; set; }
        public double AttendanceRate { get; set; }
        public int ClassSession { get; set; }
        public int UnexcusedAbsent { get; set; }
        public int Lateness { get; set; }
        public List<ExcusedAbsence> ExcusedAbsence { get; set; }
        public int PresenceCount { set; get; }
        public double PresenceRate { get; set; }
        public int Absence { get; set; }
        //public List<Workhabit> Workhabits { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Semester { get; set; }
        public string Term { get; set; }
    }
}
