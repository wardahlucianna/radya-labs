using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class SessionAttendanceV2Result
    {
        public ItemValueVm Subject { get; set; }
        public ItemValueVm Session { get; set; }
        public string ClassId { get; set; }
        public int TotalStudent { get; set; }
        public int Unsubmitted { get; set; }
        public int UnexcusedAbsence { get; set; }
        public string LastSavedBy { get; set; }
        public string IdScheduleLesson { get; set; }
        public string IdHomeroom { get; set; }
        public DateTime? LastSavedAt { get; set; }
    }
}
