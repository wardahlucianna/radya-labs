using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummary
{
    public class GetDetailAttendanceWorkhabitByStudentAndPeriodRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdStudent { get; set; }
        public string IdMappingAttendanceWorkhabit { get; set; }
    }
}
