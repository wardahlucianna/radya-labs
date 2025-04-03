using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDetailExcusedAbsentStudentAndPeriodRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdStudent { get; set; }
        public ExcusedAbsenceCategory? ExcusedAbsenceCategory { get; set; }
    }
}
