using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailWorkhabitResult : CodeWithIdVm
    {
        public DateTime Date { set; get; }
        public string Session { set; get; }
        public string Subject { set; get; }
        public string TeacherName { set; get; }
        public string HomeroomTeacherName { set; get; }
        public string Comment { set; get; }
    }
}
