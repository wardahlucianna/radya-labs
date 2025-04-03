using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.Attendance
{
    public class SessionAttendanceResult
    {
        public ItemValueVm Subject { get; set; }
        public ItemValueVm Session { get; set; }
        public string ClassId { get; set; }
        public int TotalStudent { get; set; }
        public int Pending { get; set; }
        public int Unsubmitted { get; set; }
        public int Submitted { get; set; }
        public int UnexcusedAbsence { get; set; }
        public string LastSavedBy { get; set; }
        public DateTime? LastSavedAt { get; set; }
    }
}
