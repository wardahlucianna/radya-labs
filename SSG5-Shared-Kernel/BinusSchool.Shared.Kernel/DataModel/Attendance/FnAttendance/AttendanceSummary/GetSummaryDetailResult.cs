using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public interface ISummaryDetailResult { }
    public class GetSummaryDetailResult<T> where T : ISummaryDetailResult
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public AbsentTerm Term { get; set; }
        public bool IsEAGrouped { get; set; }
        public bool IsUseDueToLateness { get; set; }
        public List<T> Data { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
