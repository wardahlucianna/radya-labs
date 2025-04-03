using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetAttendanceEntryV2Result : CodeWithIdVm
    {
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public string IdScheduleLesson { get; set; }
        public int Semester { get; set; }
        public bool UsingCheckboxAttendance { get; set; }
        public bool NeedValidation { get; set; }
        public NameValueVm Teacher { get; set; }
        public DateTime Date { get; set; }
        public string Session { get; set; }
        public AttendanceEntrySummary Summary { get; set; }
        public IEnumerable<AttendanceEntryStudent> Entries { get; set; }
        public RenderAttendance RenderAttendance { get; set; }
    }

    public class AttendanceEntrySummary
    {
        public int TotalStudent { get; set; }
        public int Pending { get; set; }
        public int Submitted { get; set; }
        public int Unsubmitted => TotalStudent - Pending - Submitted;
    }

    public class AttendanceEntryStudent : NameValueVm
    {
        public string IdScheduleLesson { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdSession { get; set; }
        public AttendanceEntryItem Attendance { get; set; }
        public List<AttendanceEntryItemWorkhabit> Workhabits { get; set; }
        public TimeSpan? Late { get; set; }
        public bool IsFromTapping { get; set; }
        public int LateInMinute => Late.HasValue ? (int)Late.Value.TotalMinutes : 0;
        public string File { get; set; }
        public string Note { get; set; }
        public AttendanceEntryStatus Status { get; set; }
        public string PositionIn { get; set; }
    }

    public class AttendanceEntryItem : CodeWithIdVm
    {
        public string IdAttendanceMapAttendance { get; set; }
        public bool IsFromAttendanceAdministration { get; set; }
    }

    public class AttendanceEntryItemWorkhabit : CodeWithIdVm
    {
        public bool IsTick { get; set; }
    }
}
