using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetDownloadAttendanceSummaryRequest
    {
        public string IdAcademicYear { get; set; }
        public string SelectedPosition { get; set; }
        public string IdUser { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int? Semester { get; set; }
        public string IdPeriod { get; set; }
        public string IsUseWorkhabit { get; set; }
        public string IsNeedValidation { get; set; }
        public string IdClassroom { get; set; }
        public string Measure { get; set; }
        public string AttendanceType { get; set; }
        public int? Percent { get; set; }
    }
}
