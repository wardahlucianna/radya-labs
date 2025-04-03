using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceBlockingSetting
{
    public class GetAttendanceBlockingSettingDetailResult : CodeWithIdVm
    {
        public MapAttendanceDetail AtdMappingAtd { get; set; }
    }

    public class MapAttendanceDetail : CodeWithIdVm
    {
        public string IdAtdMappingAtd { get; set; }
    }
}
