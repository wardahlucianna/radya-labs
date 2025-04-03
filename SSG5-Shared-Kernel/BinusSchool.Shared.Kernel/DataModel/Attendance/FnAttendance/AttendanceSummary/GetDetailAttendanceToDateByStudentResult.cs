using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDetailAttendanceToDateByStudentResult : NameValueVm
    {
        public bool IsEAGrouped { get; set; }
        public List<AttendanceToDateDetail> Details { get; set; }
        public AttendanceToDateSummary Summary { get; set; }
    }

    public class AttendanceToDateDetail
    {
        public string IdGeneratedScheduleLesson { get; set; }
        public DateTime Date { get; set; }
        public NameValueVm Session { get; set; }
        public NameValueVm Subject { get; set; }
        public NameValueVm Teacher { get; set; }
        public AttendanceToDateAttendance Attendance { get; set; }
        public string Reason { get; set; }
    }

    public class AttendanceToDateSummary
    {
        public IDictionary<string, Dictionary<string, int>> ExcusedAbsence { get; set; }
        public int UnexcusedAbsence { get; set; }
    }

    public class AttendanceToDateAttendance : CodeWithIdVm
    {
        public string IdAttendanceMapAttendance { get; set; }
        public AbsenceCategory? AbsenceCategory { get; set; }
        public ExcusedAbsenceCategory? ExcusedAbsenceCategory { get; set; }
        public CodeVm Attendance { get; set; }
    }
}
