using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class UnresolvedAttendanceResult
    {
        public DateTime Date { get; set; }
        public string ClassID { get; set; }
        public ItemValueVm Teacher { get; set; }
        public ItemValueVm Subject { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm Session { get; set; }
        public CodeWithIdVm Level { get; set; }
        public string IdStudent { get; set; }
        public List<string> IdStudentNonActive { get; set; }
    }
}
