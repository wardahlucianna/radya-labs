using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetHomeroomByIdUserRequest
    {
        public string IdAcademicYear { get; set; }
        public string SelectedPosition { get; set; }
        public string IdUser { get; set; }
        public string IdClassroom { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int? Semester { get; set; }
        public string IdHomeroom { get; set; }
        public string IdSubject { get; set; }
        public string ClassId { get; set; }
        public string IdSchool { get; set; }
    }
}
