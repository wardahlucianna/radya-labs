using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm
{
    public class GetMappingAttendance
    {
        public string IdLevel { get; set; }
        public AbsentTerm AbsentTerms { get; set; }
    }
}
