using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailResult : CodeWithIdVm
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public NameValueVm Student { get; set; }
        public NameValueVm Homeroom { get; set; }
        public double AttendanceRate { get; set; }
        public int ClassSession { get; set; }
        public int UnexcusedAbsent { get; set; }
        public int excusedAbsent { get; set; }
        public int Lateness { get; set; }
        public List<ExcusedAbsence> ExcusedAbsence { get; set; }
        public double PresenceRate { get; set; }
        public int AbsenceRate { get; set; }
        public List<Workhabit> Workhabits { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AbsenceDate { get; set; }
    }
    public class ExcusedAbsence
    {
        public ExcusedAbsenceCategory? Category { get; set; }
        public int Count { get; set; }
    }
    public class Workhabit : CodeWithIdVm
    {
        public int Count { get; set; }
    }
}
