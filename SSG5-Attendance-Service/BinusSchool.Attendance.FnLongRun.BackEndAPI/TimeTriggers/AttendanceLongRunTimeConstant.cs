using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Attendance.FnLongRun.TimeTriggers
{
    public class AttendanceLongRunTimeConstant
    {
        public const string SchoolEventConstantTime = "0 0 22 * * *"; // at 5 AM

        public const string AttendanceSummaryConstantTime = "0 0 14 * * *";  // at 9 PM

        public const string MoveStudentAttendanceConstantTime = "0 0 18 * * *";  // at 1 AM
    }
}
