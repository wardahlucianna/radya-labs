using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration
{
    public class GetAttendanceAdministrationSummaryResult
    {
        public QuotaVm Quota { get; set; }
        public Use Used { get; set; }
        public int WillUsed { get; set; }
        public RemainingAfterUsed RemainingAfterUsed { get; set; }
    }

    public class QuotaVm
    {
        public int TotalQuotaSession { get; set; }
        public int TotalStudentSession { get; set; }
    }

    public class Use
    {
        public int TotalUse { get; set; }
        public decimal PercentageUse { get; set; }
    }

    public class RemainingAfterUsed
    {
        public int TotalRemainingAfterUsed { get; set; }
        public decimal PercentageRemainingAfterUsed { get; set; }
    }
}
