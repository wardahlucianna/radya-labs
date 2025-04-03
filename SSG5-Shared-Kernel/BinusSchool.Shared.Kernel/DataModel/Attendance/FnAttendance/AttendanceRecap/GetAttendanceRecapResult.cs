using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceRecap
{
    public class GetAttendanceRecapResult : CodeWithIdVm
    {
        public string IdAcademicYear { get; set; }
        public string AcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdHomeroom { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string Homeroom { get; set; }
        public int UnSubmitted { get; set; }
        public int Pending {  get; set; }
        public int Present { get; set; }
        public int Late { get; set; }
        public int ExcusedAbsence { get; set; }
        public int UnexcusedAbsence { get; set; }

    }
}
