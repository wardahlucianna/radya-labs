using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration
{
    public class GetAttendanceAdministrationDetailResult
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
        public StudentHomeroomAttendanceAdministration Homeroom { get; set; }
        public AttendanceCategory AttendanceCategory { get; set; }
        public AbsenceCategory? AbsenceCategory { get; set; }
        public string AttendanceName { get; set; }
        public string ExcusedAbsenceCategory { get; set; }
        public string Status { get; set; }
        public bool CanApprove { get; set; }
    }

    public class StudentHomeroomAttendanceAdministration : CodeWithIdVm
    {
        public string Pathway { get; set; }
        public int Semester { get; set; }

    }
}
