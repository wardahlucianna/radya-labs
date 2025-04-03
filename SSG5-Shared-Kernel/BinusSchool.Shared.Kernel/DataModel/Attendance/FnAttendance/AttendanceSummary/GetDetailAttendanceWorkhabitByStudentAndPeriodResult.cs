using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDetailAttendanceWorkhabitByStudentAndPeriodResult
    {
        public CodeWithIdVm Student { get; set; }
        public CodeWithIdVm Workhabit { get; set; }
        public DateTime Date { get; set; }
        public string Session { get; set; }
        public string Subject { get; set; }
        public string Teacher { get; set; }
        public string Comment { get; set; }
    }
}
