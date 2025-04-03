using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailLevelResult : CodeWithIdVm
    {
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Class { get; set; }
        public CodeWithIdVm Teacher { get; set; }
        public int Unsubmited { get; set; }
        public int Pending { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdHomeroom { get; set; }
    }
}
