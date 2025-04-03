using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Quota
{
    public class QuotaResult
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public List<AttendanceQuotaVm> Quotas { get; set; }
    }

    public class AttendanceQuotaVm
    {
        public AttendanceVm Attendance { get; set; }
        public decimal Percentage { get; set; }
    }

    public class AttendanceVm : CodeWithIdVm
    {
        public AbsenceCategory? AbsenceCategory { get; set; }
        public ExcusedAbsenceCategory? ExcusedAbsenceCategory { get; set; }
    }
}
