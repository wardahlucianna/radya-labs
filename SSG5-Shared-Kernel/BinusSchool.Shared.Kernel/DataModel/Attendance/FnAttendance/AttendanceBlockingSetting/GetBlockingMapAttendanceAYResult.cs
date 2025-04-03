using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceBlockingSetting
{
    public class GetBlockingMapAttendanceAYResult : CodeWithIdVm
    {
        public IEnumerable<MapAttendanceItem> MapAttendanceItems { get; set; }
    }
    public class MapAttendanceItem : CodeWithIdVm
    {
        public string IdAtdMappingAtd { get; set; }
    }
}
