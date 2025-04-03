using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceAndWorkhabitResult : CodeWithIdVm
    {
        public string Type { get; set; }
        public AbsenceCategory? AbsenceCategory { get; set; }
        public int Total { get; set; }
        public List<GetStudentAttendance> Students { get; set; } = new List<GetStudentAttendance>();
    }

    public class GetStudentAttendance
    {
        public NameValueVm Student { get; set; }
        public string Status { get; set; }
    }
}
