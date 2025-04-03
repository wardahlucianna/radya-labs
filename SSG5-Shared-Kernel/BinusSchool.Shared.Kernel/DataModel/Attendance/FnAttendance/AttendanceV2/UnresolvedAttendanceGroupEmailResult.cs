using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class UnresolvedAttendanceGroupEmailResult
    {
        public DateTime Date { get; set; }
        public string ClassID { get; set; }
        public string IdAcademicYear { get; set; }
        public ItemValueVm Teacher { get; set; }
        public ItemValueVm Subject { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm Session { get; set; }
        public int TotalStudent { get; set; }
    }
}
