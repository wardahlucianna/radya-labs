using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class SummaryBySubjectResult : ISummaryDetailResult
    {
        public ItemValueVm Homeroom { get; set; }
        public string ClassId { get; set; }
        public ItemValueVm Subject { get; set; }
        public ItemValueVm Teacher { get; set; }
        public bool HasSession { get; set; }
        public List<UnresolvedAttendanceGroupResult> Pending { get; set; }
        public List<UnresolvedAttendanceGroupResult> Unsubmitted { get; set; }

    }
}
