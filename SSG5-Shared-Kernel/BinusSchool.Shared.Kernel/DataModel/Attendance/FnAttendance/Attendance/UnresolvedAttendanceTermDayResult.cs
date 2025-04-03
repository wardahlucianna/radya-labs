using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class UnresolvedAttendanceTermDayResult
    {
        public DateTime Date { get; set; }
        public ItemValueVm Teacher { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public int Total { get; set; }
    }
}
