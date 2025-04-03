using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class SummaryByStudentResult : ISummaryDetailResult
    {
        public ItemValueVm Student { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public double AttendanceRate { get; set; }
        public int ClassSession { get; set; }
        public int UnexcusedAbsent { get; set; }
        public int Lateness { get; set; }
        public List<ExcusedAbsence> ExcusedAbsence { get; set; }
        public double PresenceRate { get; set; }
        public int AbsenceRate { get; set; }
        public List<Workhabit> Workhabits { get; set; }
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
