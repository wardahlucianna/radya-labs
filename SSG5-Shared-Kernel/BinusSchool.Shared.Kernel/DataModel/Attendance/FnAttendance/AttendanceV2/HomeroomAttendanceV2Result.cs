using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class HomeroomAttendanceV2Result
    {
        public CodeWithIdVm Level { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public int TotalStudent { get; set; }
        public int Unsubmitted { get; set; }
        public int UnexcusedAbsence { get; set; }
        public string LastSavedBy { get; set; }
        public DateTime? LastSavedAt { get; set; }
    }
}
