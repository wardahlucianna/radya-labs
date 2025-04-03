using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDashboardDetailParentRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string IdAttendance { get; set; }
        public string PeriodType { get; set; }
        public string IdLevel { get; set; }
    }
}
