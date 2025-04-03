using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class RedisAttendanceSummaryMappingAttendanceResult
    {
        public string Id { get; set; }
        public string IdLevel { get; set; }
        public bool IsNeedValidation { get; set; }
        public bool IsUseDueToLateness { get; set; }
        public bool IsUseWorkhabit { get; set; }
        public AbsentTerm AbsentTerms { get; set; }

    }
}
