using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryBySchoolDayRequest
    {
        public string IdScheduleLesson { get; set; }
        public string Status { get; set; }
    }
}
