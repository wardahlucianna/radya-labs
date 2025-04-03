using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class SummaryByLevelResult : ISummaryDetailResult
    {
        public CodeWithIdVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm Teacher { get; set; }
        public int Pending { get; set; }
        public int Unsubmitted { get; set; }

    }
}
