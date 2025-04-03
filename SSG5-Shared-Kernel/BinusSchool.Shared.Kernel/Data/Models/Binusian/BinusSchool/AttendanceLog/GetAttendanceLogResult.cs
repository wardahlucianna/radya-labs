using System;
using System.Collections.Generic;
using BinusSchool.Data.Apis.Binusian;

namespace BinusSchool.Data.Models.Binusian.BinusSchool.AttendanceLog
{
    public class GetAttendanceLogResult : BinusianApiResult
    {
        public List<AttendanceLog> AttendanceLogResponse { get; set; }
    }

    public class AttendanceLog
    {
        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public string BinusianID { get; set; }
        public string BinusianName { get; set; }
        public DateTime DetectedDate { get; set; }
    }
}
