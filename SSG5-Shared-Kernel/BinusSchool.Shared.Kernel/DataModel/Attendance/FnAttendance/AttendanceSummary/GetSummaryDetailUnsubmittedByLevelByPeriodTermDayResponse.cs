using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetSummaryDetailUnsubmittedByLevelByPeriodTermDayResponse
    {
        public DateTime ScheduleDate { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm Teacher { get; set; }
        public int TotalAttendance { get; set; }
    }
}
