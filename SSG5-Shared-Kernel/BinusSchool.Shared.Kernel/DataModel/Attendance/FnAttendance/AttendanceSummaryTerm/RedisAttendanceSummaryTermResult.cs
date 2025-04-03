using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class RedisAttendanceSummaryTermResult
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public RedisAttendanceSummaryStudent Student { get; set; }
        public string AttendanceWorkhabitName { get; set; }
        public TrAttendanceSummaryTermType AttendanceWorkhabitType { get; set; }
        public int Total { get; set; }
    }
}
