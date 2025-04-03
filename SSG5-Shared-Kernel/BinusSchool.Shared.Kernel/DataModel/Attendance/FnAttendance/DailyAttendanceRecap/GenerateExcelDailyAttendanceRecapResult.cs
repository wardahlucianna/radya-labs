using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.DailyAttendanceRecap
{
    public class GenerateExcelDailyAttendanceRecapResult
    {
        public byte[] ExcelOutput { get; set; }
        public string FileName { get; set; }
    }
}
