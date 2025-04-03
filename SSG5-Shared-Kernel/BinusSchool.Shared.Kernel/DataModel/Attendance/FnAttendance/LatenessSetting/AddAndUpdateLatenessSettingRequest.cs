using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.LatenessSetting
{
    public class AddAndUpdateLatenessSettingRequest
    {
        public string IdLevel { get; set; }
        public PeriodType Period { get; set; }
        public int TotalLate { get; set; }
        public int TotalUnexcusedAbsend { get; set; }
    }
}
