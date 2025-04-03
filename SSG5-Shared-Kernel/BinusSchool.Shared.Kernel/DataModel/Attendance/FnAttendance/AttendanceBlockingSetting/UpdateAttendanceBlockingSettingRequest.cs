using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceBlockingSetting
{
    public class UpdateAttendanceBlockingSettingRequest
    {
        public string IdBlockingType { get; set; }
        public string IdAcademicYear { get; set; }
        public List<MapBlockingAttendanceSetting> MapBlockingAttendanceSettings { get; set; }
    }

    public class MapBlockingAttendanceSetting 
    {
        public string IdLevel { get; set; }
        public string IdAtdMappingAtd { get; set; }
    }
}
