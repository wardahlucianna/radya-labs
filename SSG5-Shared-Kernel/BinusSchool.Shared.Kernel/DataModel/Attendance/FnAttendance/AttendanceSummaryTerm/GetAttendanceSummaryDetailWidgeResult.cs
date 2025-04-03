using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailWidgeResult
    {
        public int TotalStudent { get; set; }
        public int Pending { get; set; }
        public int Present { get; set; }
        public int Late { get; set; }
        public int ExcusedAbsence { get; set; }
        public int UnxcusedAbsence { get; set; }
        public int Unsubmited { get; set; }
        public bool IsUseDueToLateness { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool IsEaGroup { get; set; }
        public bool IsNeedValidation { get; set; }
        public bool IsUseWorkhabit { get; set; }
        public AbsentTerm AbsentTerm { get; set; }
    }
}
