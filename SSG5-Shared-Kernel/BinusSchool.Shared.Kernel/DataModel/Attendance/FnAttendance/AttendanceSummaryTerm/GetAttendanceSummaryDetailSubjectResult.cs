using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryDetailSubjectResult : CodeWithIdVm
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public string IdHomeroom { get; set; }
        public string Homeroom { get; set; }
        public string ClassIdSubject { get; set; }
        public string TeacherName { get; set; }
        public int Unsubmited { get; set; }
        public int Pending { get; set; }
    }
}
