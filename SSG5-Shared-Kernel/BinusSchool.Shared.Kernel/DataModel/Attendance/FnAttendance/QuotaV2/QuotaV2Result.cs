using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.QuotaV2
{
    public class QuotaV2Result
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public List<QuotaDetailVm> QuotaDetails { get; set; }
    }

    public class QuotaDetailVm
    {
        public AttendanceCategory AttendanceCategory { get; set; }
        public AbsenceCategory? AbsenceCategory { get; set; }
        public ExcusedAbsenceCategory? ExcusedAbsenceCategory { get; set; }
        public AttendanceStatus Status { get; set; }
        public decimal Percentage { get; set; }
    }
}
