using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDashboardRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }

        /// <summary>
        /// type period : All,Term,Semeter
        /// </summary>
        public string PeriodType { get; set; }
    }
}
