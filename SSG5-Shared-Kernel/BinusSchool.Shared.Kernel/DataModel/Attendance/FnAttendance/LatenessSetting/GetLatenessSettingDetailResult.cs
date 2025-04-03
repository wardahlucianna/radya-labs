using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.LatenessSetting
{
    public class GetLatenessSettingDetailResult
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Level { get; set; }
        public PeriodType Period { get; set; }
        public int TotalLate { get; set; }
        public int TotalUnexcusedAbsend { get; set; }
    }
}
