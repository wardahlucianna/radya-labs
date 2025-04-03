using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryResult : CodeWithIdVm
    {
        public NameValueVm AcademicYear { get; set; }
        public NameValueVm Level { get; set; }
        public int Submited { get; set; }
        public int Unsubmitted { get; set; }
        public int Pending { get; set; }
        public int TotalStudent { get; set; }
        public bool IsNeedValidation { get; set; }
        public bool IsUseWorkhabit { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Enddate { get; set; }
        public AbsentTerm AbsentTerm { get; set; }
    }
}
