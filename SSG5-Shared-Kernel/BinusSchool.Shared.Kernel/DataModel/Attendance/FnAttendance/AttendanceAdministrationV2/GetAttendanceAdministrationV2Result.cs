using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class GetAttendanceAdministrationV2Result : CodeWithIdVm
    {
        public CodeWithIdVm Student { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public int Semester { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
        public CodeWithIdVm Attendance { get; set; }
        public string Detail { get; set; }
        public DateTime? SubmittedDate { get; set; }
        
    }
}
