using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDownloadAllUnexcusedAbsenceByRangeResult
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string Class { get; set; }
        public DateTime DateOfAbsence { get; set;}
        public int ClassSession { get; set; }
        public string Subject { get; set; }
        public string TeacherName { get; set; }
        public string AttendanceStatus { get; set; }
    }
}
