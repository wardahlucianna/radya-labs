using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.QuotaV2
{
    public class SetQuotaV2Request
    {
        public string IdLevel { get; set; }
        public string IdAcademicYear { get; set; }
        public List<QuotaDetailsRequest> QuotaDetails { get; set; }
    }

    public class QuotaDetailsRequest
    {
        public AttendanceCategory AttendanceCategory { get; set; }
        public AbsenceCategory? AbsenceCategory { get; set; }
        public ExcusedAbsenceCategory? ExcusedAbsenceCategory { get; set; }
        public AttendanceStatus Status { get; set; }
        public decimal Percentage { get; set; }
    }
}
