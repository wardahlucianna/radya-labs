using System;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDetailExcusedAbsentStudentRequest
    {
        public string IdAcademicYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdStudent { get; set; }
        public ExcusedAbsenceCategory? ExcusedAbsenceCategory { get; set; }
    }
}
