using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class SummaryResult
    {
        public DateTimeRange Period { get; set; }
        public int? Semester { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public AbsentTerm AbsentTerm { get; set; }
        public bool IsNeedValidation { get; set; }
        public int TotalStudent { get; set; }
        public int Submitted { get; set; }
        public int Pending { get; set; }
        public int Unsubmitted { get; set; }
    }
}
