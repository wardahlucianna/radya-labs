using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetAttendanceRateByStudentResult
    {
        public ItemValueVm Subject { get; set; }
        public int ClassSession { get; set; }
        public int Present { get; set; }
        public int ExcusedAbsent { get; set; }
        public int UnexcusedAbsent { get; set; }
        public int Late { get; set; }
        public double PresenceRate { get; set; }
        public double PunctualityRate { get; set; }
    }
}
