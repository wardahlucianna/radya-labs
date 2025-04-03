using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailUnexcusedExcusedSummaryResult
    {
        public AbsenceCategory? AbsenceCategory { get; set; }
        public List<GetAttendance> Attendance { get; set; }
    }

    public class GetAttendance : ItemValueVm
    {
        public int Total { get; set; }
    }
}
