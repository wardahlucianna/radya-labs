using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailAttendanceRateResult : CodeWithIdVm
    {
        public string Subject { get; set; }
        public int ClassSessionToDate { get; set; }
        public int TotalDaysToDate { get; set; }
        public int Present { get; set; }
        public int UnexcusedAbsence { get; set; }
        public int ExcusedAbsence { get; set; }
        public int Late { get; set; }
        public double PresenceInClass { get; set; }
        public double Punctuality { get; set; }
    }
}
