using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.ApprovalAttendanceAdministration
{
    public class GetListAttendanceAdministrationApprovalResult : CodeWithIdVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm ClassHomeroom { get; set; }
        public CodeWithIdVm Student { get; set; }
        public CodeWithIdVm Attendance { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public bool CanApprove { get; set; }
        
    }
}
