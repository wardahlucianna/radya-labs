using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class GetAttendanceAdministrationDetailV2Result
    {
        public CodeWithIdVm Student { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public int Semester { get; set; }
        public CodeWithIdVm ClassHomeroom { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartPeriod { get; set; }
        public TimeSpan EndPeriod { get; set; }
        public CodeWithIdVm Attendance { get; set; }
        public StudentHomeroomAttendanceAdministrationV2 Homeroom { get; set; }
        public AttendanceCategory AttendanceCategory { get; set; }
        public AbsenceCategory? AbsenceCategory { get; set; }
        public string AttendanceName { get; set; }
        public string ExcusedAbsenceCategory { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public bool CanApprove { get; set; }
        public List<CancelAttendance> CancelAttendances { get; set; }
    }

    public class StudentHomeroomAttendanceAdministrationV2 : CodeWithIdVm
    {
        public string Pathway { get; set; }
        public int Semester { get; set; }

    }

    public class CancelAttendance
    {
        public DateTime Date { get; set; }
        public TimeSpan StartDate { get; set; }
        public TimeSpan EndDate { get; set; }
        public string SessionID { get; set; }
    }
}
