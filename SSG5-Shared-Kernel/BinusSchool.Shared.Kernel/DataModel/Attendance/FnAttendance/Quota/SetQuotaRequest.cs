using System.Collections.Generic;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Quota
{
    public class SetQuotaRequest
    {
        public string IdLevel { get; set; }
        public List<QuotaRequest> Quotas { get; set; }
    }

    public class QuotaRequest
    {
        public string IdAttendance { get; set; }
        public decimal Percentage { get; set; }
    }
}
