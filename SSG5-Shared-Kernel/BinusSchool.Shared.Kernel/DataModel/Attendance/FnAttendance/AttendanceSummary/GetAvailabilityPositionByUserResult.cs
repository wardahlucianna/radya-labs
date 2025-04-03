using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetAvailabilityPositionByUserResult
    {
        public IReadOnlyList<ItemValueVm> ClassAdvisors { get; set; }
        public IReadOnlyList<ItemValueVm> SubjectTeachers { get; set; }
        public IReadOnlyList<OtherPositionResult> OtherPositions { get; set; }
    }

    public class OtherPositionResult : CodeWithIdVm
    {
        public string Data { get; set; }
    }
}
