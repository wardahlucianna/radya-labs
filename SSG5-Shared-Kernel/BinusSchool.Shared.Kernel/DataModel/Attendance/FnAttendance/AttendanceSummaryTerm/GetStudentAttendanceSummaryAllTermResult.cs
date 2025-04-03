using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetStudentAttendanceSummaryAllTermResult
    {
        public NameValueVm Student { get; set; }
        //public NameValueVm Homeroom { get; set; }
        public double AttendanceRate { get; set; }
        public int ClassSession { get; set; }
        public int UnexcusedAbsent { get; set; }
        public int Lateness { get; set; }
        public List<ExcusedAbsence> ExcusedAbsence { get; set; }
        public int PresenceCount { set; get; }
        public int SickCount { set; get; }
        public double PresenceRate { get; set; }
        public int Absence { get; set; }
        //public List<Workhabit> Workhabits { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Semester { get; set; }
        public string Term { get; set; }
        public int SickCountInDays { set; get; }
        public List<ExcusedAbsence> ExcusedAbsenceInDays { get; set; }
        public int UnexcusedAbsentInDays { get; set; }
    }
}
