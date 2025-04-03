using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class SummaryBySchoolDayResult : ISummaryDetailResult
    {
        public ItemValueVm Homeroom { get; set; }
        public string DayOfWeek { get; set; }
        public DateTime Date { get; set; }
        public List<Session> Sessions { get; set; }
    }
    public class Session : ItemValueVm
    {
        public string ClassId { get; set; }
        public List<AttendanceData> Attendances { get; set; }
        public List<AttendanceData> Workhabits { get; set; }
        public List<ItemValueVm> Unsubmitted { get; set; }
        public List<ItemValueVm> Pending { get; set; }
    }
    public class AttendanceData : CodeWithIdVm
    {
        public List<ItemValueVm> Students { get; set; }
    }
}
