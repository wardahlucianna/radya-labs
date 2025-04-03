using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Scheduling.FnLongRun.TimeTriggers
{
    public class ScheduleLongRunTimerConstant
    {
        public const string EventSchoolConstantTime = "0 0 18 * * *"; // 1 AM on server

        public const string EventSchoolHalfConstantTimeHalfDay = "0 0 5 * * *"; // 12 PM on server
    }
}
