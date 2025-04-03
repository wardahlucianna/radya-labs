using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Attendance.FnAttendance.Models
{
    public class AttendanceEntryNewResult
    {
        public string IdAttendanceEntry { get; set; }
        public string IdScheduleLesson { get; set; }
        public string IdHomeroomStudent { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string IdLesson { get; set; }
        public string ClassID { get; set; }
        public AttendanceSummarySessionResult Session { get; set; }
        public CodeWithIdVm Classroom { get; set; }
        public string IdGrade { get; set; }
        public string GradeCode { get; set; }
        public string IdDay { get; set; }
        public string IdWeek { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public AttendanceSummarySubjectResult Subject { get; set; }
        public string IdStudent { get; set; }
        public AttendanceEntryStatus Status { get; set; }
        public AttendanceSummaryAttendanceResult Attendance { get; set; }
        public string Notes { get; set; }
        public string IdUserTeacher { get; set; }
        public StudentResult Student { get; set; }
        public string IdAttendanceMappingAttendance { get; set; }
        public int Semester { get; set; }
        public List<AttendanceEntryWorkhabitResult> AttendanceEntryWorkhabit { get; set; }
        public string IdUserAttendanceEntry { get; set; }
        public bool IsFromAttendanceAdministration { get; set; }
        public DateTime DateIn { get; set; }
        public string IdHomeroom { get; set; }
        public string Homeroom { get; set; }
    }
}
