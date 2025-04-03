using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration
{
    public class GetAttendanceAdministrationResult : CodeWithIdVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Student { get; set; }
        public CodeWithIdVm Attendance { get; set; }
        public string Detail { get; set; }
        public DateTime? SubmittedDate { get; set; }
        
    }
}
