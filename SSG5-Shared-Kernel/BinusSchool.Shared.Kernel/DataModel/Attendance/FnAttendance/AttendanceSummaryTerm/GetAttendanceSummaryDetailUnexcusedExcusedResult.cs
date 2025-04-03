using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailUnexcusedExcusedResult : CodeWithIdVm
    {
        public DateTime Date { set; get; }
        public string Session { set; get; }
        public int SessionID { set; get; }
        public string Subject { set; get; }
        public string TeacherName { set; get; }
        public string HomeroomTeacherName { set; get; }
        public string AttendanceStatus { set; get; }
        public string IdAttendance { set; get; }
        public string Reason { set; get; }
        public AbsenceCategory? AbsenceCategory { set; get; }
        
    }

}
